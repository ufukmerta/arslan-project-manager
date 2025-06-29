using ArslanProjectManager.Core;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.DeleteDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.ViewModels;
using AutoMapper;

namespace ArslanProjectManager.Service.Mappings
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<BoardTagDto, BoardTag>().ReverseMap();
            CreateMap<LogCategoryDto, LogCategory>().ReverseMap();
            CreateMap<ProjectDto, Project>().ReverseMap();
            CreateMap<ProjectTaskDto, ProjectTask>().ReverseMap();
            CreateMap<RoleDto, Role>().ReverseMap();
            CreateMap<TaskCategoryDto, TaskCategory>().ReverseMap();
            CreateMap<TaskCommentDto, TaskComment>().ReverseMap();
            CreateMap<TaskLogDto, TaskLog>().ReverseMap();
            CreateMap<TaskTagDto, TaskTag>().ReverseMap();
            CreateMap<TeamDto, Team>().ReverseMap();
            CreateMap<TeamInviteDto, TeamInvite>().ReverseMap();
            CreateMap<TeamUserDto, TeamUser>().ReverseMap();
            CreateMap<UserDto, User>().ReverseMap();
            CreateMap<User, MiniUserDto>().ReverseMap();
            CreateMap<TokenDto, Token>().ReverseMap();

            CreateMap<BoardTagUpdateDto, BoardTag>();
            CreateMap<LogCategoryUpdateDto, LogCategory>();
            CreateMap<ProjectUpdateDto, Project>();
            CreateMap<ProjectTaskUpdateDto, ProjectTask>();
            CreateMap<RoleUpdateDto, Role>();
            CreateMap<TaskCategoryUpdateDto, TaskCategory>();
            CreateMap<TaskCommentUpdateDto, TaskComment>();
            CreateMap<TaskLogUpdateDto, TaskLog>();
            CreateMap<TaskTagUpdateDto, TaskTag>();
            CreateMap<TeamUpdateDto, Team>();
            CreateMap<TeamInviteUpdateDto, TeamInvite>();
            CreateMap<TeamUserUpdateDto, TeamUser>();
            CreateMap<User, UserCreateDto>()
                .ForMember(src => src.ProfilePicture, dst => dst.Ignore());
            CreateMap<UserCreateDto, User>()
                .ForMember(src => src.ProfilePicture, dst => dst.Ignore());
            CreateMap<Token, TokenDto>().ReverseMap();

            CreateMap<UserUpdateDto, User>()
                .ForMember(src => src.ProfilePicture, dst => dst.Ignore());
            CreateMap<User, UserUpdateDto>()
                .ForMember(src => src.ProfilePicture, dst => dst.Ignore());

            //WEBSITE VIEW MODELS
            //User
            //User/Profile
            CreateMap<UserProfileDto, UserProfileViewModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => src.ProfilePicture))
                .ForMember(dest => dest.RegisterDate, opt => opt.MapFrom(src => src.RegisterDate))
                .ForMember(dest => dest.OwnProfile, opt => opt.MapFrom(src => src.OwnProfile))
                .ForMember(dest => dest.TotalProjects, opt => opt.MapFrom(src => src.TotalProjects))
                .ForMember(dest => dest.CompletedProjects, opt => opt.MapFrom(src => src.CompletedProjects))
                .ForMember(dest => dest.CurrentTeam, opt => opt.MapFrom(src => src.CurrentTeam))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role)).ReverseMap();

            //User/Login
            CreateMap<UserLoginDto, LoginViewModel>().ReverseMap();
        }
    }
}
