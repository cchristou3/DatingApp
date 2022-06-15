using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        protected static ILogger _logger = NLog.LogManager.LoadConfiguration("NLog.config").GetCurrentClassLogger();
        public readonly PresenceTracker _presenceTracker;
        public readonly IHubContext<PresenceHub> _presenceHub;

        public MessageHub(
            IUnitOfWork unitOfWork, IMapper mapper,
            PresenceTracker presenceTracker, IHubContext<PresenceHub> presenceHub
        )
        {
            _presenceHub = presenceHub;            
            _presenceTracker = presenceTracker;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.Info("MessageHub.OnConnectedAsync");

            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            // Send the group to the related clients. It will be used to identify
            // when another client has joined the chat
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            if(_unitOfWork.HasChanges()) await _unitOfWork.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.Info("MessageHub.OnDisconnectedAsync");
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username.Equals(createMessageDto.RecipientUsername, StringComparison.OrdinalIgnoreCase))
                throw new HubException("You cannot send messages to yourself");


            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("Recipient not found");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = username,
                RecipientUsername = createMessageDto.RecipientUsername,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

            // If the user is connected to the same chat
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await _presenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    // The user is online, but not in the chat (same group)
                    await _presenceHub.Clients
                    .Clients(connections)
                    .SendAsync("NewMessageReceived", new
                    {
                        Username = sender.UserName,
                        KnownAs = sender.KnownAs
                    });
                }
            }

            await _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<Message, MessageDto>(message));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _unitOfWork.Complete()) return group;

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _unitOfWork.MessageRepository.RemoveConnection(connection);

            if (await _unitOfWork.Complete()) return group;

            throw new HubException("Failed to leave group");
        }

        private string GetGroupName(string caller, string other)
        {
            var isCallerSmaller = string.CompareOrdinal(caller, other) < 0;
            return isCallerSmaller ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}