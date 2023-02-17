using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;

namespace API.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;

    public MessageService(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public Message AddMessage(CreateMessageDto createMessageDto, AppUser sender, AppUser recipient)
    {
        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        _messageRepository.AddMessage(message);
        return message;
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        return await _messageRepository.GetMessagesForUser(messageParams);
    }

    public async Task<bool> SaveAllMessagesAsync()
    {
        return await _messageRepository.SaveAllAsync();
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string username, string currentUsername)
    {
        return await _messageRepository.GetMessageThread(currentUsername, username);
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _messageRepository.GetMessage(id);
    }

    public void DeleteMessage(Message message)
    {
        _messageRepository.DeleteMessage(message);
    }

    public void DeleteSenderOrRecipientMessage(Message message, string username)
    {
        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;
    }

    public void AddGroup(Group group)
    {
        _messageRepository.AddGroup(group);
    }

    public void RemoveConnection(Connection connection)
    {
        _messageRepository.RemoveConnection(connection);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
       return await _messageRepository.GetConnection(connectionId);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await _messageRepository.GetMessageGroup(groupName);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _messageRepository.GetMessageGroup(connectionId);
    }
}