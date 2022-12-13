using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;

namespace API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }
    
    public async Task SetDefaultGenderFilter(UserParams userParams, string username)
    {
        var currentUser = await _userRepository.GetUserByUsernameAsync(username);
        userParams.CurrentUsername = currentUser.UserName;

        if (string.IsNullOrEmpty(userParams.Gender))
        {
            userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
        }
    }
    
    public async Task UpdateUser(MemberUpdateDto memberUpdateDto, string username)
    {
        var user = await _userRepository.GetUserByUsernameAsync(username);

        _mapper.Map(memberUpdateDto, user);
        _userRepository.Update(user);
    }
    
    public async Task<PagedList<MemberDto>> GetMembers(UserParams userParams)
    {
        return await _userRepository.GetMembersAsync(userParams);
    }
    
    public async Task<MemberDto> GetMember(string username)
    {
        return await _userRepository.GetMemberAsync(username);
    }
    
    public async Task<AppUser> GetUserByUsername(string username)
    {
        return  await _userRepository.GetUserByUsernameAsync(username);
    }

    public async Task<bool> SaveAllUserAsync()
    {
        return await _userRepository.SaveAllAsync();
    }
}