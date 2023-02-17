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
    private readonly IHubContext<PresenceHub> _presenceHub;

    public MessageHub(
        IMessageRepository messageRepository,
        IUserService userService,
        IMessageService messageService,
        IMapper mapper,
        IHubContext<PresenceHub> presenceHub)
    {
        _messageRepository = messageRepository;
        _userService = userService;
        _messageService = messageService;
        _mapper = mapper;
        _presenceHub = presenceHub;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null)
        {
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _messageRepository
                .GetMessageThread(Context.User.GetUsername(), otherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var (senderUsername, recipientUsername) = ValidateSenderAndRecipient(createMessageDto);
        var sender = await GetSender(senderUsername);
        var recipient = await GetRecipient(recipientUsername);
        var message = _messageService.AddMessage(createMessageDto, sender, recipient);

        var groupName = GetGroupName(senderUsername, recipientUsername);
        var group = await _messageService.GetMessageGroup(groupName);
        await UpdateDateRead(group, message, recipientUsername, sender);

        await SaveChangesAndNotifyRecipients(groupName, message);
    }

    private async Task UpdateDateRead(Group group, Message message, string recipientUsername, AppUser sender)
    {
        if (group.Connections.Any(x => x.Username == recipientUsername))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionForUser(recipientUsername);
            if (connections is not null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                    new {username = sender.UserName, knownAs = sender.KnownAs});
            }
        }
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

    private async Task SaveChangesAndNotifyRecipients(string groupName, Message message)
    {
        if (await _messageService.SaveAllMessagesAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
        }
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var group = await _messageService.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

        if (group is null)
        {
            group = new Group(groupName);
            _messageService.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to add to group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await _messageService.GetGroupForConnection(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        _messageService.RemoveConnection(connection);


        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to remove from group");
    }
}