using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;

namespace API.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task SetDefaultGenderFilter(UserParams userParams, string username)
    {
        var gender = await _unitOfWork.UserRepository.GetUserGender(username);
        userParams.CurrentUsername = username;

        if (string.IsNullOrEmpty(userParams.Gender))
        {
            userParams.Gender = gender == "male" ? "female" : "male";
        }
    }

    public async Task UpdateUser(MemberUpdateDto memberUpdateDto, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

        _mapper.Map(memberUpdateDto, user);
        _unitOfWork.UserRepository.Update(user);
    }

    public async Task<PagedList<MemberDto>> GetMembers(UserParams userParams)
    {
        return await _unitOfWork.UserRepository.GetMembersAsync(userParams);
    }

    public async Task<MemberDto> GetMember(string username, bool isCurrentUser)
    {
        return await _unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser);
    }

    public async Task<AppUser> GetUserByUsername(string username)
    {
        return await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
    }

    public async Task<bool> SaveAllUserAsync()
    {
        return await _unitOfWork.Complete();
    }

    public async Task<AppUser> GetUserByPhotoId(int id)
    {
        return await _unitOfWork.UserRepository.GetUserByPhotoId(id);
    }
}