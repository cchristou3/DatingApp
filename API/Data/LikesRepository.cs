using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            Console.WriteLine("LikesRepository");
            _context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
        {
            // Here we start with all users and build the queryable up from there.
            // If this method is called from the controller /likes/ without any precicate it will return all users.
            var users = _context.Users.AsQueryable();
            var likes = _context.Likes.AsQueryable();

            // All the users that have liked our current user
            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
                // users variable is replected here...
                users = likes.Select(like => like.SourceUser);
            }

            // All users that have been liked by our current user
            else if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                // users variable is replected here...
                users = likes.Select(like => like.LikedUser);
            }

            return await PagedList<LikeDTO>.CreateAsync(users.OrderBy(u => u.UserName).Select(user => new LikeDTO
            {
                Id = user.Id,
                Username = user.UserName,
                Age = user.DateOfBirth.CalculateAge(),
                KnownAs = user.KnownAs,
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City
            }), likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                                .Include(x => x.LikedByUsers)
                                .Include(x => x.LikedUsers)
                                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}