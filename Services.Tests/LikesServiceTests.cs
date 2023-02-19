using System;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
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
    private Mock<IUnitOfWork> _unitOfWork;

    [SetUp]
    public void SetUp()
    {
        // Set up test data here, e.g.
        _sourceUser = new AppUser {Id = 1};
        _likedUser = new AppUser {Id = 2};
        _likesParams = new LikesParams();
        _unitOfWork = new Mock<IUnitOfWork>();
        _likesService = new LikesService(_unitOfWork.Object);
    }


    [Test]
    public async Task TestGetUsersWithLikes_ValidInput_ReturnsExpectedUser()
    {
        // Arrange
        const int sourceUserId = 1;
        var expectedUser = new AppUser {Id = 1, UserName = "John"};

        _unitOfWork.Setup(uow => uow.LikesRepository.GetUsersWithLikes(sourceUserId))
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
        _unitOfWork.Setup(uow => uow.LikesRepository.GetUsersWithLikes(invalidUser))
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
        _unitOfWork.Setup(uow => uow.LikesRepository.GetUserLike(1, 2))
            .ReturnsAsync(new UserLike {SourceUserId = 1, TargetUserId = 2});
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
        var invalidLikedUser = new AppUser {Id = 4};

        //Act
        var result = await _likesService.GetUserLike(-1, invalidLikedUser);
        Console.WriteLine("Null?");
        Console.WriteLine( 1 == 1);
        var new_result = await _likesService.GetUserLike(-1, invalidLikedUser);

        //Assert
        Assert.IsNull(result);
        Assert.IsNull(new_result);
    }

    [Test]
    public async Task TestGetUsersLikes_ReturnsCorrectPageSize()
    {
        var likes = Enumerable.Range(1, 10)
            .Select(i => new UserLike {SourceUserId = _sourceUser.Id, TargetUserId = i})
            .ToList();

        _unitOfWork.Setup(uow => uow.LikesRepository.GetUsersLikes(_likesParams))
            .ReturnsAsync(new PagedList<LikeDto>(
                likes.Select(l => new LikeDto {Id = l.TargetUserId, UserName = $"User{l.TargetUserId}"}),
                10, 1, 1));

        // Act
        var result = await _likesService.GetUsersLikes(_likesParams);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(10, result.Count);
        Assert.AreEqual(10, result.TotalCount);
        Assert.AreEqual(10, result.TotalPages);
    }
}