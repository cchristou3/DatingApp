using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
                src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
                
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<Photo, PhotoForApprovalDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.AppUser.KnownAs))
                .ForMember(dest => dest.PhotoId, opt => opt.MapFrom(src => src.Id));


            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderPhotoUrl, opt => opt.MapFrom(src =>
                src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.RecipientPhotoUrl, opt => opt.MapFrom(src =>
                src.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));
            
            //* Concept:
            //* To avoid any date times mismatches, we are going to use DateTime.UtcNow.
            //* This will ensure that all clients have the same date times.
            //* We just will need to add the 'Z' at the end of it, so that the client will know
            //* that it is a Utc Time, and to convert it to its local time zone.
            // When we return the dates to the client, they are going to have that 'Z' on the end of it
            // CreateMap<DateTime, DateTime>()
            // .ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            //* Commented out because we are using ApplyUtcDateTimeConverter instead.
        }
    }
}