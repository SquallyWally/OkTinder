using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUserService _userService;

        public UsersController( IMapper mapper, IPhotoService photoService, IUserService userService)
        {
            _mapper = mapper;
            _photoService = photoService;
            _userService = userService;
        }
        
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            await _userService.SetDefaultGenderFilter(userParams, User.GetUsername());
          
            var users = await _userService.GetMembers(userParams);

            Response.AddPaginationHeader(new PaginationHeader(
                users.CurrentPage,
                users.PageSize,
                users.TotalCount,
                users.TotalPages));
            
            return Ok(users);
        }

        
        // api/users/{username}
        [Authorize(Roles = "Member")]
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var retrievedUser = _userService.GetMember(username);
            return await Task.FromResult<ActionResult>(Ok(retrievedUser));
        }
        
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            //get the name from the token
            await _userService.UpdateUser(memberUpdateDto, User.GetUsername());

            if (await _userService.SaveAllUserAsync()) return NoContent();
            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userService.GetUserByUsername(User.GetUsername());
            
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = _photoService.AddPhotoToUser(result, user);

            if (await _userService.SaveAllUserAsync())
            {
                return CreatedAtRoute("GetUser", new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("We have a problem while adding a photo");
        }
        

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userService.GetUserByUsername(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo is {IsMain: true}) return BadRequest("This is already your main photo");
            _photoService.IsPhotoMain(user, photo);

            if (await _userService.SaveAllUserAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }
        

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userService.GetUserByUsername(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            switch (photo)
            {
                case null:
                    return NotFound();
                case {IsMain: true}:
                    return BadRequest("This is already your main photo");
            }

            if (photo.PublicId is not null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);
            if (await _userService.SaveAllUserAsync()) return Ok();
            return BadRequest("Failed to delete the photo");
        }
    }
}