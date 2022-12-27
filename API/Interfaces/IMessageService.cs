using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageService
{
    Message AddMessage(CreateMessageDto createMessageDto, AppUser sender, AppUser recipient);
    Task<bool> SaveAllMessagesAsync();
    Task<IEnumerable<MessageDto>> GetMessageThread(string username, string currentUsername);
    Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
    Task<Message> GetMessage(int id);
    void DeleteMessage(Message message);
    void DeleteSenderOrRecipientMessage(Message message, string username);
}