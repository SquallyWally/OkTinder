using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;

namespace API.Services;

public class LikesService : ILikesService
{
    private readonly IUnitOfWork _unitOfWork;

    public LikesService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppUser> GetUsersWithLikes(int sourceUserId)
    {
        return await _unitOfWork.LikesRepository.GetUsersWithLikes(sourceUserId);
    }

    public async Task<UserLike> GetUserLike(int sourceUserId, AppUser likedUser)
    {
        return await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);
    }

    public async Task<PagedList<LikeDto>> GetUsersLikes(LikesParams likesParams)
    {
        return await _unitOfWork.LikesRepository.GetUsersLikes(likesParams);
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