namespace API.SignalR;

public class PresenceTracker
{
    public static readonly Dictionary<string, List<string>> OnlineUsers = new();

    public Task UserConnected(string username, string connectionId)
    {
        // Put a lock because a Dict is not thread safe. So with lock we make it un-async
        lock (OnlineUsers)
        {
            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username].Add(connectionId);
            }
            else
            {
                OnlineUsers.Add(username, new List<string> {connectionId});
            }
        }

        return Task.CompletedTask;
    }

    public Task UserDisconnected(string username, string connectionId)
    {
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(username)) return Task.CompletedTask;
            OnlineUsers[username].Remove(connectionId);

            if (OnlineUsers[username].Count == 0)
            {
                OnlineUsers.Remove(username);
            }
        }

        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers;
        lock (OnlineUsers)
        {
            onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
;        }

        return Task.FromResult(onlineUsers);
    }
}