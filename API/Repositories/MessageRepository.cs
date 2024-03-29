﻿using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.Messages.Remove(message);
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = _context.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(r => r.RecipientUsername == messageParams.Username && r.RecipientDeleted == false),
            "Outbox" => query.Where(s => s.SenderUsername == messageParams.Username && s.SenderDeleted == false),
            _ => query.Where(r =>
                r.RecipientUsername == messageParams.Username && r.RecipientDeleted == false && r.DateRead == null)
        };

        var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

        return await PagedList<MessageDto>
            .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        var query = await GetMessages(currentUsername, recipientUsername);
        var unreadMessages = query.Where(d => d.DateRead == null
                                              && d.RecipientUsername == currentUsername).ToList();

        if (!unreadMessages.Any())
        {
            foreach (var message in unreadMessages)
            {
                message.DateRead = DateTime.Now;
            }
        }

        return await query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void AddGroup(Group group)
    {
        _context.Groups.Add(group);
    }

    public void RemoveConnection(Connection connection)
    {
        _context.Connections.Remove(connection);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await _context.Groups
            .Include(x => x.Connections) // Eager loading
            .SingleOrDefaultAsync(x => x.Name == groupName);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _context.Groups
            .Include(c => c.Connections)
            .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }

    private Task<IQueryable<Message>> GetMessages(string currentUsername, string recipientUsername)
    {
        var query = _context.Messages
            .Where(
                u => u.RecipientUsername == currentUsername && u.RecipientDeleted == false &&
                     u.SenderUsername == recipientUsername ||
                     u.RecipientUsername == recipientUsername && u.SenderDeleted == false &&
                     u.SenderUsername == currentUsername
            )
            .OrderBy(m => m.MessageSent)
            .AsQueryable();
        return Task.FromResult(query);
    }
}