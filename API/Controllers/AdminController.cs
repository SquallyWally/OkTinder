using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
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

        var addResult  = await _userManager.AddToRolesAsync(user, selectedRoles.Except((userRoles)));
        if (!addResult.Succeeded) return BadRequest(addResult.Errors);

        var removeResult = await _userManager.RemoveFromRolesAsync(user, selectedRoles.Except(selectedRoles));
        if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);

        return Ok(await _userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {
        return Ok("Admins or mods can see this");
    }
}