using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly ILikesService _likesService;

        public LikesController(IUserService userService, ILikesService likesService)
        {
            _userService = userService;
            _likesService = likesService;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        { 
            var likedUser = await _userService.GetUserByUsername(username);
            if (likedUser == null) return NotFound();
            
            var sourceUserId = User.GetUserId();
            var sourceUser = await _likesService.GetUsersWithLikes(sourceUserId);
            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself.");

            var userLike = await _likesService.GetUserLike(sourceUserId, likedUser);
            if (userLike != null) return BadRequest("You already liked this user");

            _likesService.AddUserLike(sourceUserId, likedUser, sourceUser, out userLike);

            if (await _userService.SaveAllUserAsync()) return Ok();

            return BadRequest("Failed to like user");
        }

        
        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesService.GetUsersLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount,
                users.TotalPages));
            return Ok(users);
        }
    }
}