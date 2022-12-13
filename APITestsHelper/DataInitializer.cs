using API.Entities;

namespace APITestHelper;

public class DataInitializer
{
    public static List<AppUser>? GetAllUsers()
    {
        var users = new List<AppUser>
        {
            new AppUser
            {
                UserName = "Bob",
                Gender = "male",
                DateOfBirth = DateTime.Now,
                Created = DateTime.Now,
                LastActive = DateTime.Now
            },
            new AppUser
            {
                UserName = "Groot",
                Gender = "male",
                DateOfBirth = DateTime.Now,
                Created = DateTime.Now,
                LastActive = DateTime.Now
            }, 
            new AppUser
            {
                UserName = "Tifa",
                Gender = "female",
                DateOfBirth = DateTime.Now,
                Created = DateTime.Now,
                LastActive = DateTime.Now
            },
        };
        return users;
    }
}