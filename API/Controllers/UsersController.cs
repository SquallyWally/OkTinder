using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;

        // Get data from db via dep inj
        public UsersController(DataContext context)
        {
            _context = context;
        }

        // api/users/{id}
        [HttpGet("{id}")]
      
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            // todo: maak het naar async
            var user = await _context.Users.FindAsync(id);
            return user;
        }
        // List is overkill, IEnum is genoeg voor dit
        public async Task< ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }
    }
}
