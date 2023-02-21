using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class PhotoRepository : IPhotoRepository
{
    private readonly DataContext _context;

    public PhotoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
    {
        return await _context.Photos
            .IgnoreQueryFilters()
            .Where(x => x.isApproved == false)
            .Select(p => new PhotoForApprovalDto
            {
                Id = p.Id,
                Url = p.Url,
                Username = p.AppUser.UserName,
                isApproved = p.isApproved
            }).ToListAsync();
    }

    public async Task<Photo> GetPhotoById(int id)
    {
        return await _context.Photos.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public void RemovePhoto(Photo photo)
    {
        _context.Photos.Remove(photo);
    }
}