﻿using API.Entities;
using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Length <= 0) return uploadResult;
        
        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, stream),
            Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
        };
        uploadResult = await _cloudinary.UploadAsync(uploadParams);

        return uploadResult;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        
        return await _cloudinary.DestroyAsync(deleteParams);
    }
    
    public Photo AddPhotoToUser(ImageUploadResult result, AppUser user)
    {
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count == 0)
        {
            photo.IsMain = true;
        }

        user.Photos.Add(photo);
        return photo;
    }
    
    public void IsPhotoMain(AppUser user, Photo photo)
    {
        var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
        if (currentMain is not null) currentMain.IsMain = false;
        if (photo != null) photo.IsMain = true;
    }
}