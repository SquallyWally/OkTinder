using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Repositories;
using API.Services;
using APITestHelper;
using AutoMapper;
using Moq;
using NUnit.Framework;

namespace Services.Tests;

public class UserServiceTests
{
    private IUserService? _userService;
    private List<AppUser>? _users;
    private IUserRepository? _userRepository;
    private readonly IMapper _mapper;


    [SetUp]
    public void Setup()
    {
        _users = SetUpUsers();
        _userRepository = SetUpRepository();
        _userService = new UserService(_userRepository, _mapper);
    }

    private IUserRepository SetUpRepository()
    {
        var mockRepo = new Mock<IUserRepository>();

        mockRepo.Setup(m => m.GetUsersAsync()).ReturnsAsync(_users);

        // mockRepo.Setup(m => m.GetUserByIdAsync(It.IsAny<int>()))
        //     .Returns(new Func<int, AppUser>(id => _users.Find(m => m.Id.Equals(id))));

        return mockRepo.Object;
    }


    private static List<AppUser>? SetUpUsers()
    {
        var userId = new int();
        var users = DataInitializer.GetAllUsers();
        foreach (var user in users!)
        {
            user.Id = ++userId;
        }

        return users;
    }

    [TearDown]
    public void DisposeTest()
    {
        _users = null;
        _userService = null;
        _userRepository = null;
    }

    [Test]
    public void GetMembersTest()
    {
        //Assert.Pass();
        var userParams = new UserParams
        {
            CurrentUsername = "Douwe", Gender = "Male"
        };
        
        var users = _userService.GetMembers(userParams);
        Assert.IsNotNull(users);
    }
}