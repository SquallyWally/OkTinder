using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class Group
{
    public Group(string name)
    {
        Name = name;
    }

    // Empty ctor for EF. Cause it does not expect to pass the parameters
    public Group()
    {
    }

    [Key] 
    public string Name { get; set; }

    public ICollection<Connection> Connections { get; set; } = new List<Connection>();
}