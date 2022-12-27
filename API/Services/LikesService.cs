using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;

namespace API.Services;

public class LikesService : ILikesService
{
    private readonly ILikesRepository _likesRepository;

    public LikesService(ILikesRepository likesRepository)
    {
        _likesRepository = likesRepository;
    }

    public async Task<AppUser> GetUsersWithLikes(int sourceUserId)
    {
        return await _likesRepository.GetUsersWithLikes(sourceUserId);
    }

    public async Task<UserLike> GetUserLike(int sourceUserId, AppUser likedUser)
    {
        return await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);
    }

    public async Task<PagedList<LikeDto>> GetUsersLikes(LikesParams likesParams)
    {
        return await _likesRepository.GetUsersLikes(likesParams);
    }

    public void AddUserLike(int sourceUserId, AppUser likedUser, AppUser sourceUser, out UserLike userLike)
    {
        userLike = new UserLike
        {
            SourceUserId = sourceUserId,
            TargetUserId = likedUser.Id
        };

        sourceUser.LikedUsers.Add(userLike);
    }
}