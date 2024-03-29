﻿using API.Entities;
using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
    Photo AddPhotoToUser(ImageUploadResult result, AppUser user);
    void IsPhotoMain(AppUser user, Photo photo);
}