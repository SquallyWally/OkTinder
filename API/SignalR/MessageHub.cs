using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub : Hub
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserService _userService;
    private readonly IMessageService _messageService;
    private readonly IMapper _mapper;

    public MessageHub(IMessageRepository messageRepository, IUserService userService, IMessageService messageService,
        IMapper mapper)
    {
        _messageRepository = messageRepository;
        _userService = userService;
        _messageService = messageService;
        _mapper = mapper;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext != null)
        {
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var messages = await _messageRepository
                .GetMessageThread(Context.User.GetUsername(), otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var (senderUsername, recipientUsername) = ValidateSenderAndRecipient(createMessageDto);
        var sender = await GetSender(senderUsername);
        var recipient = await GetRecipient(recipientUsername);
        var message = _messageService.AddMessage(createMessageDto, sender, recipient);
        await SaveChangesAndNotifyRecipients(message, sender, recipient);
    }

    private async Task<AppUser> GetSender(string senderUsername)
    {
        return await _userService.GetUserByUsername(senderUsername);
    }


    private async Task<AppUser> GetRecipient(string recipientUsername)
    {
        var recipient = await _userService.GetUserByUsername(recipientUsername);
        if (recipient is null) throw new HubException("Recipient not found");

        return recipient;
    }

    private (string senderUsername, string recipientUsername) ValidateSenderAndRecipient(
        CreateMessageDto createMessageDto)
    {
        var senderUsername = Context.User.GetUsername();
        var recipientUsername = createMessageDto.RecipientUsername;
        if (senderUsername == recipientUsername.ToLower())
            throw new HubException("You cannot send messages to yourself");

        return (senderUsername, recipientUsername);
    }

    private async Task SaveChangesAndNotifyRecipients(Message message, AppUser sender, AppUser recipient)
    {
        if (await _messageService.SaveAllMessagesAsync())
        {
            var group = GetGroupName(sender.UserName, recipient.UserName);
            await Clients.Group(group).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
        }
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}