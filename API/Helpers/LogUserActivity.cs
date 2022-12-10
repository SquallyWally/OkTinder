using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Wait when the api has done its job, then we do our thing
        var resultContext = await next();

        if (resultContext.HttpContext.User.Identity is {IsAuthenticated: false}) return;

        var userId = resultContext.HttpContext.User.GetUserId();

        var repository = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repository.GetUserByIdAsync(int.Parse(userId));
        user.LastActive =  DateTime.UtcNow;
        await repository.SaveAllAsync();

    }
}