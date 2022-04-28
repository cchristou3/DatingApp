using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        protected static ILogger _logger = NLog.LogManager.LoadConfiguration("NLog.config").GetCurrentClassLogger();
        
        private readonly PresenceTracker _presenceTracker;
        public PresenceHub(PresenceTracker presenceTracker)
        {
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {   
            _logger.Info("PresenceHub.OnConnectedAsync");

            var isOnline = await _presenceTracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);

            // A user may be connected from multiple devices
            // Thus, we want to execute the below method, only on the user's first logon
            if (isOnline)
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
            
            var currentUsers = await _presenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.Info("PresenceHub.OnDisconnectedAsync");

            var isOffline = await _presenceTracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
            // A user may be connected to multiple devices
            // Thus, we want to execute the below method, only if he has disconnected from all the devices
            if (isOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            await base.OnDisconnectedAsync(exception);
        }
    }
}