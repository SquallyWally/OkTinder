using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesService
{
    Task<AppUser> GetUsersWithLikes(int sourceUserId);
    Task<UserLike> GetUserLike(int sourceUserId, AppUser likedUser);
    Task<PagedList<LikeDto>> GetUsersLikes(LikesParams likesParams);
    void AddUserLike(int sourceUserId, AppUser likedUser, AppUser sourceUser, out UserLike userLike);
}