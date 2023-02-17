namespace API.Entities;

public class Connection
{
    public Connection(string connectionId, string username)
    {
        ConnectionId = connectionId;
        Username = username;
    }

    // Empty ctor for EF. Cause it does not expect to pass the parameters
    public Connection()
    {
    }

    public string ConnectionId { get; set; }
    public string Username { get; set; }
}