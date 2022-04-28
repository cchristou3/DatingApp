using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    /*
    TODO: Scaling out presence tracker 
    * Integrate with Redis
    Some starting points:
    "https://www.codemag.com/article/1807061/Build-Real-time-Applications-with-ASP.NET-Core-SignalR"
    "https://docs.microsoft.com/en-us/aspnet/core/signalr/scale?view=aspnetcore-5.0#redis-backplane"
    "https://docs.microsoft.com/en-us/aspnet/core/signalr/redis-backplane?view=aspnetcore-5.0"
    */
    public class PresenceTracker
    {
        private static readonly IDictionary<string, List<string>> OnlineUsers =
                                            new ConcurrentDictionary<string, List<string>>();

        public Task<bool> UserConnected(string username, string connectionId)
        {   
            bool isOnline = false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, new List<string> { connectionId });
                    isOnline = true;
                }

                return Task.FromResult(isOnline);
            }
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

                OnlineUsers[username].Remove(connectionId);
                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                };

                return Task.FromResult(isOffline);
            }
        }

        public Task<string[]> GetOnlineUsers()
        {
            lock (OnlineUsers)
            {
                var onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(x => x.Key).ToArray();
                return Task.FromResult(onlineUsers);
            }
        }

        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock (OnlineUsers)
            {
                OnlineUsers.TryGetValue(username, out connectionIds);
            }

            return Task.FromResult(connectionIds);
        }
    }
}