using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public PhotoRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Photo> GetPhotoById(int photoId)
        {
            return await _context.Photos.IgnoreQueryFilters()
            .AsTracking().SingleOrDefaultAsync(x => x.Id == photoId);
        }

        public async Task<List<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            return await _context.Photos.AsNoTracking().IgnoreQueryFilters().Where(x => !x.IsApproved).ProjectTo<PhotoForApprovalDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<bool> RemovePhoto(int photoId)
        {
            var photo = await _context.Photos.IgnoreQueryFilters().AsTracking().SingleOrDefaultAsync(x => x.Id == photoId);
            if (photo == null) return false;
            _context.Photos.Remove(photo);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}