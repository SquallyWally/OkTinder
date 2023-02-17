using System;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Moq;
using NUnit.Framework;

namespace Services.Tests;

public class LikesServiceTests
{
    private ILikesService _likesService;
    private AppUser _sourceUser;
    private AppUser _likedUser;
    private LikesParams _likesParams;
    private Mock<ILikesRepository> _likesRepository;
    private UserLike _userLike;


    [SetUp]
    public void SetUp()
    {
        // Set up test data here, e.g.
        _sourceUser = new AppUser {Id = 1};
        _likedUser = new AppUser {Id = 2};
        _likesParams = new LikesParams();
        _userLike = new UserLike {SourceUserId = 1, TargetUserId = 2};
        _likesRepository = new Mock<ILikesRepository>();
        _likesService = new LikesService(_likesRepository.Object);
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
        _likesParams.PageSize = 5;

        // Act
        var result = await _likesService.GetUsersLikes(_likesParams);

        // Assert
        Assert.AreEqual(5, result.PageSize);
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