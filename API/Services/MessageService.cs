using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;

namespace API.Services;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;

    public MessageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

        _unitOfWork.MessageRepository.AddMessage(message);
        return message;
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        return await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
    }

    public async Task<bool> SaveAllMessagesAsync()
    {
        return await _unitOfWork.Complete();
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string username, string currentUsername)
    {
        return await _unitOfWork.MessageRepository.GetMessageThread(currentUsername, username);
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _unitOfWork.MessageRepository.GetMessage(id);
    }

    public void DeleteMessage(Message message)
    {
        _unitOfWork.MessageRepository.DeleteMessage(message);
    }

    public void DeleteSenderOrRecipientMessage(Message message, string username)
    {
        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;
    }

    public void AddGroup(Group group)
    {
        _unitOfWork.MessageRepository.AddGroup(group);
    }

    public void RemoveConnection(Connection connection)
    {
        _unitOfWork.MessageRepository.RemoveConnection(connection);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _unitOfWork.MessageRepository.GetConnection(connectionId);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _unitOfWork.MessageRepository.GetGroupForConnection(connectionId);
    }
}