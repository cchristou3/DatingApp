using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        public IUnitOfWork _uow { get; }
        public AdminController(UserManager<AppUser> userManager, IUnitOfWork uow)
        {
            _uow = uow;
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            _logger.Info("POST Admin.GetUsersWithRoles");
            var users = await _userManager.Users
            .AsNoTracking()
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .OrderBy(x => x.UserName)
            .Select(x => new
            {
                x.Id,
                UserName = x.UserName,
                Roles = x.UserRoles.Select(y => y.Role.Name).ToList()
            })
            .ToListAsync();

            return Ok(users);
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<IActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            _logger.Info("POST Admin.EditRoles");
            var selectedRoles = roles.Split(',').ToList();

            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return NotFound("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(selectedRoles.OrderBy(x => x));
        }

        [Authorize(Policy = "ModeratePhotosRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            _logger.Info("GET Admin.GetPhotosForModeration");
            return Ok(await _uow.PhotoRepository.GetUnapprovedPhotos());
        }

        [Authorize(Policy = "ModeratePhotosRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<IActionResult> ApprovePhoto(int photoId)
        {
            _logger.Info("POST Admin.ApprovePhoto");

            var photoToApprove = await _uow.PhotoRepository.GetPhotoById(photoId);
            if (photoToApprove == null) return NotFound("Photo not found.");
            photoToApprove.IsApproved = true;
            
            var user = await _uow.UserRepository.GetUserByPhotoId(photoId);
            // If the user does not have any photo set as its main, then set this one as its main.
            if (user.Photos.All(x => !x.IsMain))
            {
                photoToApprove.IsMain = true;
            }

            if (await _uow.Complete()) return Ok(photoId);
            
            return BadRequest("Failed to approve the photo.");
        }

        [Authorize(Policy = "ModeratePhotosRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            _logger.Info("POST Admin.RejectPhoto");

            if (await _uow.PhotoRepository.RemovePhoto(photoId)) return Ok(photoId);
            return BadRequest("Failed to reject the photo.");
        }

    }
}
