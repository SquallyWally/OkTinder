using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IUserService
{ 
    Task SetDefaultGenderFilter(UserParams userParams, string username);
    Task UpdateUser(MemberUpdateDto memberUpdateDto, string username);
    Task<PagedList<MemberDto>> GetMembers(UserParams userParams);
    Task<MemberDto> GetMember(string username);
    Task<AppUser> GetUserByUsername(string username);
    Task<bool> SaveAllUserAsync();
}