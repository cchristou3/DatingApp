using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            Console.WriteLine("UserRepository");
            _mapper = mapper;
            _context = context;
        }

        public async Task<MemberDto> GetMemberByUsernameAsync(string username, string currentLoggedInUsername)
        {
            var query = _context.Users.Where(x => x.UserName == username.ToLower());

            if (username.EqualsIgnoreCase(currentLoggedInUsername)) query = query.IgnoreQueryFilters();

            return await query
                        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                        .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            var query = _context.Users
                                .AsNoTracking()
                                .Where(x =>
                                x.UserName != userParams.CurrentUsername &&
                                x.Gender == userParams.Gender &&
                                x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

            var columnToOrderBy = userParams.OrderBy;
            query = query.OrderBy(columnToOrderWith: columnToOrderBy, ascending: false);

            return await PagedList<MemberDto>
                        .CreateAsync(
                            query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider),
                             userParams.PageNumber,
                              userParams.PageSize
                        );
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByPhotoId(int photoId)
        {
            return await _context.Users.IgnoreQueryFilters().Include(x => x.Photos).SingleOrDefaultAsync(x => x.Photos.Any(y => y.Id == photoId));
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(x => x.Photos).SingleOrDefaultAsync(x => x.UserName == username.ToLower());
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users
            .Where(x => x.UserName == username)
            .Select(x => x.Gender)
            .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(x => x.Photos).ToListAsync();
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}