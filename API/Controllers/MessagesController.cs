using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IMessageService _messageService;
    private readonly IMapper _mapper;

    // GET
    public MessagesController(IMapper mapper, IUserService userService, IMessageService messageService)
    {
        _mapper = mapper;
        _userService = userService;
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();
        if (username == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You can't send messages to yourself.");

        var sender = await _userService.GetUserByUsername(username);
        var recipient = await _userService.GetUserByUsername(createMessageDto.RecipientUsername);

        if (recipient == null) return NotFound();

        var message = _messageService.AddMessage(createMessageDto, sender, recipient);

        if (await _messageService.SaveAllMessagesAsync()) return Ok(_mapper.Map<MessageDto>(message));

        return BadRequest("Something went wrong while sending a message.");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await _messageService.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount,
            messages.TotalPages));

        return messages;
    }
    
    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();
        return Ok(await _messageService.GetMessageThread(username, currentUsername));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        var message = await _messageService.GetMessage(id);

        if (message.SenderUsername != username && message.RecipientUsername != username)
            return Unauthorized();

        _messageService.DeleteSenderOrRecipientMessage(message, username);

        if (message.SenderDeleted && message.RecipientDeleted)
        {
            _messageService.DeleteMessage(message);
        }

        if (await _messageService.SaveAllMessagesAsync()) return Ok();

        return BadRequest("Problem with deleting the message");
    }
}