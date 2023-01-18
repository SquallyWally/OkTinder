using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Moq;
using NUnit.Framework;

namespace Services.Tests;

public class LikesServiceTests
{
    private ILikesService _likesService;
    private AppUser _sourceUser;
    private AppUser _likedUser;
    private Mock<ILikesRepository> _likesRepository;
    private UserLike _userLike;
    private IEnumerable<UserLike> _testLikes;
    private IMapper _mapper;


    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg => { cfg.CreateMap<UserLike, LikeDto>(); });
        _mapper = config.CreateMapper();
        _sourceUser = new AppUser {Id = 1};
        _likedUser = new AppUser {Id = 2};
        _userLike = new UserLike {SourceUserId = 1, TargetUserId = 2};
        _likesRepository = new Mock<ILikesRepository>();
        _likesService = new LikesService(_likesRepository.Object);
        _testLikes = new List<UserLike>
        {
            new() {SourceUserId = 1, TargetUserId = 2},
            new() {SourceUserId = 1, TargetUserId = 2}
        };
    }


    [Test]
    public async Task TestGetUsersWithLikes_ValidInput_ReturnsExpectedUser()
    {
        // Arrange
        const int sourceUserId = 1;
        var expectedUser = new AppUser {Id = 1, UserName = "John"};
        _likesRepository.Setup(repo => repo.GetUsersWithLikes(sourceUserId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _likesService.GetUsersWithLikes(sourceUserId);

        // Assert
        Assert.AreEqual(expectedUser, result);
    }
    
    [Test]
    public async Task TestGetUsersWithLikes_InvalidUserId_ReturnsNull()
    {
        // Arrange
        const int invalidUser = -1;
        _likesRepository.Setup(repo => repo.GetUsersWithLikes(invalidUser))
            .ReturnsAsync((AppUser) null!);

        // Act
        var result = await _likesService.GetUsersWithLikes(invalidUser);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task TestGetUserLike_ValidInput_ReturnsExpectedUserLike()
    {
        // Act
        _likesRepository.Setup(repo => repo.GetUserLike(1, 2))
            .ReturnsAsync(new UserLike { SourceUserId = 1, TargetUserId = 2 });
        var result = await _likesService.GetUserLike(1, _likedUser);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.SourceUserId);
        Assert.AreEqual(_likedUser.Id, result.TargetUserId);
    }

    [Test]
    public async Task TestGetUserLike_InvalidInput_ReturnsNull()
    {
        // Arrange
        var invalidLikedUser = new AppUser { Id = 4 };
        
        //Act
        var result = await _likesService.GetUserLike(-1, invalidLikedUser);
        
        //Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task TestGetUsersLikes_ReturnsCorrectPageSize()
    {
        // Arrange
        var likesParams = new LikesParams {UserId = _sourceUser.Id, Predicate = "liked"};
        var likeDtos = _mapper.Map<IEnumerable<LikeDto>>(_testLikes);
        var pagedList = new PagedList<LikeDto>(likeDtos, 1, 1, 1);
        _likesRepository
            .Setup(x => x.GetUsersLikes(likesParams))
            .ReturnsAsync(pagedList);
        
        // Act
        var result = await _likesService.GetUsersLikes(likesParams);
        
        // Assert
        Assert.AreEqual(1, result.PageSize);
    }
    
    [Test]
    public async Task TestGetUsersLikes_ReturnsPagedListsOfObjects()
    {
        // Arrange
        var likesParams = new LikesParams {UserId = _sourceUser.Id, Predicate = "liked"};
        var likeDtos = _mapper.Map<IEnumerable<LikeDto>>(_testLikes);
        var pagedList = new PagedList<LikeDto>(likeDtos, 1, 1, 1);
        _likesRepository
            .Setup(x => x.GetUsersLikes(likesParams))
            .ReturnsAsync(pagedList);
        
        // Act
        var result = await _likesService.GetUsersLikes(likesParams);
        
        // Assert
        
    }
    
}


// GetUsersWithLikes method:
//
// Test case: Verify that the method returns the expected user when given a valid sourceUserId.
// Test case: Verify that the method returns a null value when given an invalid sourceUserId.
//
// GetUserLike method:
//
// Test that the method returns the correct UserLike object when given valid input.
//     Test that the method returns null when given an invalid sourceUserId.
//     Test that the method returns null when given an invalid targetUserId.
//     Test that the method throws an exception when the database is not available.
//     Test that the method returns null when there is no matching UserLike object in the database.
//
// GetUsersLikes method:
//
// Test case: Verify that the method returns a PagedList of LikeDto objects when given valid likesParams.
// Test case: Verify that the method returns an empty PagedList when given invalid likesParams.
//
// AddUserLike method:
//
// Test case: Verify that the method correctly adds a UserLike object to the sourceUser.LikedUsers collection when given valid input.
// Test case: Verify that the userLike output parameter is correctly set when given valid input.