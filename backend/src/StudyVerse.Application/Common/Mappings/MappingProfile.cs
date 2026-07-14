using AutoMapper;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Common.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserSummaryDto>();
        CreateMap<User, UserProfileDto>();
    }
}
