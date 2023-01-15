using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using AutoMapper;

namespace Services.Tests;

using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Moq;
using NUnit.Framework;

public class MessageServicesTests
{
    private IMessageService _messageService;
    private Mock<IMessageRepository> _mockMessageRepository;
    private AppUser _sender;
    private AppUser _recipient;
    private IEnumerable<Message> _testMessages;
    private CreateMessageDto _createMessageDto;
    private IMapper _mapper;


    [SetUp]
    public void SetUp()
    {
        _mockMessageRepository = new Mock<IMessageRepository>();
        var config = new MapperConfiguration(cfg => { cfg.CreateMap<Message, MessageDto>(); });
        _mapper = config.CreateMapper();
        _messageService = new MessageService(_mockMessageRepository.Object);
        _sender = new AppUser {UserName = "sender"};
        _recipient = new AppUser {UserName = "recipient"};
        _createMessageDto = new CreateMessageDto {Content = "Hello, how are you?"};
        _testMessages = new List<Message>
        {
            new() {RecipientUsername = "user1", RecipientDeleted = false},
            new() {RecipientUsername = "user1", RecipientDeleted = false},
            new() {SenderUsername = "user1", SenderDeleted = false},
            new() {SenderUsername = "user1", SenderDeleted = false},
            new() {SenderUsername = "user2", SenderDeleted = true}
        };
    }

    [Test]
    public void AddMessage_Adds_Message_To_Repository()
    {
        // Act
        var message = _messageService.AddMessage(_createMessageDto, _sender, _recipient);

        // Assert
        Assert.AreEqual(_sender, message.Sender);
        Assert.AreEqual(_sender.UserName, message.SenderUsername);
        Assert.AreEqual(_recipient, message.Recipient);
        Assert.AreEqual(_recipient.UserName, message.RecipientUsername);
        Assert.AreEqual(_createMessageDto.Content, message.Content);
    }

    [Test]
    public async Task SaveAllMessagesAsync_Calls_SaveAllAsync_On_Repository()
    {
        // Arrange
        _mockMessageRepository.Setup(x => x.SaveAllAsync()).ReturnsAsync(true);

        // Act
        var result = await _messageService.SaveAllMessagesAsync();
        _mockMessageRepository.Verify(x => x.SaveAllAsync(), Times.Once);
        
        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task GetMessageThead_Calls_GetMessageThread_On_Repository()
    {
        // Arrange
        var expectedThread = new List<MessageDto>
        {
            new() {Content = "Hello"},
            new() {Content = "How are you?"}
        };

        _mockMessageRepository
            .Setup(x => x.GetMessageThread("currentUsername", "username"))
            .ReturnsAsync(expectedThread);

        // Act
        var result = await _messageService.GetMessageThread("username", "currentUsername");
        _mockMessageRepository.Verify(x => x.GetMessageThread("currentUsername", "username"), Times.Once);
        
        // Assert
        Assert.AreEqual(expectedThread, result);
    }

    [Test]
    public async Task GetMessagesForUser_Returns_Inbox_Messages_When_Container_Is_Inbox()
    {
        // Arrange
        var messageParams = new MessageParams {Container = "Inbox", Username = "user1"};
        var messages = _testMessages.Where(m => m.RecipientUsername == "user1" && m.RecipientDeleted == false)
            .AsEnumerable();
        var messageDtos = _mapper.Map<IEnumerable<MessageDto>>(messages);


        var pagedList = new PagedList<MessageDto>(messageDtos, 2, 1, 2);
        _mockMessageRepository
            .Setup(x => x.GetMessagesForUser(messageParams))
            .ReturnsAsync(pagedList);


        // Act
        var result = await _messageService.GetMessagesForUser(messageParams);

        // Assert
        Assert.AreEqual(2, result.TotalCount);
        Assert.AreEqual(1, result.CurrentPage);
        Assert.AreEqual(2, result.PageSize);
        Assert.AreEqual(1, result.TotalPages);
        Assert.IsTrue(result.All(m => m.RecipientUsername == "user1"));
    }

    [Test]
    public async Task GetMessagesForUser_Returns_Inbox_Messages_When_Container_Is_Outbox()
    {
        // Arrange
        var messageParams = new MessageParams {Container = "Outbox", Username = "user1"};
        var messages = _testMessages.Where(m => m.SenderUsername == "user1" && m.SenderDeleted == false)
            .AsEnumerable();
        
        var messageDtos = _mapper.Map<IEnumerable<MessageDto>>(messages);


        var pagedList = new PagedList<MessageDto>(messageDtos, 3, 1, 2);
        _mockMessageRepository
            .Setup(x => x.GetMessagesForUser(messageParams))
            .ReturnsAsync(pagedList);

        // Act
        var result = await _messageService.GetMessagesForUser(messageParams);

        // Assert
        Assert.AreEqual(3, result.TotalCount);
        Assert.AreEqual(1, result.CurrentPage);
        Assert.AreEqual(2, result.PageSize);
        Assert.AreEqual(2, result.TotalPages);
        Assert.IsTrue(result.All(m => m.SenderUsername == "user1"));
    }

    [Test]
    public async Task GetMessagesForUser_Returns_Unread_Messages_When_Container_Is_Unread()
    {
        // Arrange
        var messageParams = new MessageParams {Container = "Unread", Username = "user1"};
        var messages = _testMessages.Where(m => m.RecipientUsername == "user1" && m.RecipientDeleted == false)
            .AsEnumerable();
        var messageDtos = _mapper.Map<IEnumerable<MessageDto>>(messages);
        var pagedList = new PagedList<MessageDto>(messageDtos, 1, 1, 2);
        _mockMessageRepository
            .Setup(x => x.GetMessagesForUser(messageParams))
            .ReturnsAsync(pagedList);
        
        // Act
        var result = await _messageService.GetMessagesForUser(messageParams);
        
        // Assert
        Assert.AreEqual(1, result.TotalCount);
        Assert.AreEqual(1, result.CurrentPage);
        Assert.AreEqual(2, result.PageSize);
        Assert.AreEqual(1, result.TotalPages);
        Assert.IsTrue(result.All(m => m.RecipientUsername == "user1" && m.DateRead == null));
    }
}