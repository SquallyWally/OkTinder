using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);

        Task<AppUser> GetUsersWithLikes(int userId);

        // Predicate: Do you want the users you liked or the users liked BY ?
        Task<PagedList<LikeDto>> GetUsersLikes(LikesParams likesParams);
    }
}