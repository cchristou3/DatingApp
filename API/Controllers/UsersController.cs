using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using API.Extensions;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DataContext _context;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService, DataContext context)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
        }

        // api/users
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            _logger.Info("GET Users.GetUsers");

            var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }

            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }

        // api/users/1        
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            _logger.Info("GET Users.GetUser");
            return await _unitOfWork.UserRepository.GetMemberByUsernameAsync(username, User.GetUsername());
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            _logger.Info("PUT Users.UpdateUser");
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            _mapper.Map(memberUpdateDto, user);

            _unitOfWork.UserRepository.Update(user);

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            _logger.Info("POST Users.AddPhoto");
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                IsApproved = false,
            };

            user.Photos.Add(photo);
            _unitOfWork.UserRepository.Update(user);
            if (await _unitOfWork.Complete())
            {
                // return CreatedAtRoute("GetUser", _mapper.Map<PhotoDto>(photo));
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            _logger.Info("PUT Users.SetMainPhoto");
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            var newMainPhoto = user.Photos.SingleOrDefault(x => x.Id == photoId);
            if (newMainPhoto.IsMain) return BadRequest("This is already your main photo");
            var currentMainPhoto = user.Photos.SingleOrDefault(x => x.IsMain);
            if (currentMainPhoto != null) currentMainPhoto.IsMain = false;
            newMainPhoto.IsMain = true;

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            _logger.Info("DELETE Users.DeletePhoto");
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            var photoToBeDeleted = user.Photos.SingleOrDefault(x => x.Id == photoId);
            if (photoToBeDeleted == null) return NotFound("Photo does not exist");
            if (photoToBeDeleted.IsMain) return BadRequest("You cannot delete your main photo");

            if (photoToBeDeleted.PublicId != null)
            {
                var deletionResult = await _photoService.DeletePhotoAsync(photoToBeDeleted.PublicId);
                if (deletionResult.Error != null) return BadRequest(deletionResult.Error);
            }
            user.Photos.Remove(photoToBeDeleted);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Photo couldn't be deleted");
        }

        [HttpGet("propertyName/{propertyName}")]
        public async Task<ActionResult> GetUserProperty(string propertyName)
        {

            dynamic list = null;

            var propertyType = typeof(AppUser).GetProperty(propertyName).PropertyType;
            var propertyTypeName = propertyType.Name;

            Console.WriteLine("==================================");
            // Console.WriteLine(type);

            var users = _context.Users.AsNoTracking();

            switch (propertyTypeName)
            {
                case "String":
                    list = await users.Select<AppUser, String>(propertyName).ToListAsync();
                    break;
                case "DateTime":
                    list = await users.Select<AppUser, DateTime>(propertyName).ToListAsync();
                    break;
                case "Boolean":
                    list = await users.Select<AppUser, bool>(propertyName).ToListAsync();
                    break;
                case "Int32":
                    list = await users.Select<AppUser, int>(propertyName).ToListAsync();
                    break;
                default:
                    if (propertyTypeName.Contains("ICollection", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var propertyTypeGenericArgument = propertyType.GenericTypeArguments.FirstOrDefault();
                        switch (propertyTypeGenericArgument.Name)
                        {
                            case "Photo":
                                list = await users.Select<AppUser, ICollection<Photo>>(propertyName).ToListAsync();
                                break;
                            case "Message":
                                list = await users.Select<AppUser, ICollection<Message>>(propertyName).ToListAsync();
                                break;
                        }

                    }
                    break;
            }

            return Ok(list);
        }
    }
}