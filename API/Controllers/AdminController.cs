using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IPhotoService _photoService;
    private readonly IUserService _userService;

    public AdminController(UserManager<AppUser> userManager, IPhotoService photoService, IUserService userService)
    {
        _userManager = userManager;
        _photoService = photoService;
        _userService = userService;
    }


    [Authorize(Policy = "RequiredAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequiredAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

        var user = await _userManager.FindByNameAsync(username);

        if (user is null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = roles.Split(",").ToArray();

        var addResult = await _userManager.AddToRolesAsync(user, selectedRoles.Except((userRoles)));
        if (!addResult.Succeeded) return BadRequest(addResult.Errors);

        var removeResult = await _userManager.RemoveFromRolesAsync(user, selectedRoles.Except(selectedRoles));
        if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);

        return Ok(await _userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult> GetPhotosForModeration()
    {
        var unapprovedPhotos = await _photoService.GetUnapprovedPhotos();

        return Ok(unapprovedPhotos);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        var photo = await _photoService.GetPhotoById(photoId);
        photo.isApproved = true;

        await _photoService.Complete();
        return Ok();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var photo = await _photoService.GetPhotoById(photoId);
        if (photo.PublicId is not null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Result == "ok") _photoService.RemovePhoto(photo);
        }
        else
        {
            _photoService.RemovePhoto(photo);
        }

        await _photoService.Complete();
        return Ok();
    }

    public async Task<ActionResult> CheckMain(int photoId)
    {
        var photo = await _photoService.GetPhotoById(photoId);
        if (photo is null) return NotFound("There is no photo.");
        photo.isApproved = true;
        
        var user = await _userService.GetUserByPhotoId(photo.Id);

        if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;
        
        await _photoService.Complete();

        return Ok();
    }
}