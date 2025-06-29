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
            CreateMap<UserLoginDto, LoginViewModel>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password)).ReverseMap();

            //User/Register
            CreateMap<UserCreateDto, RegisterViewModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore()).ReverseMap();

            //User/EditProfile
            CreateMap<UserUpdateDto, EditUserViewModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => src.ProfilePicture))
                .ForMember(dest => dest.ProfilePictureFile, opt => opt.Ignore()).ReverseMap();//profile picture is handled separately: file -> base64

            //User/ChangePassword
            CreateMap<UserPasswordUpdateDto, ChangePasswordViewModel>()
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.NewPassword, opt => opt.MapFrom(src => src.NewPassword))
                .ForMember(dest => dest.ConfirmNewPassword, opt => opt.Ignore()).ReverseMap();

            //Home
            //Home/Index2
            CreateMap<HomeDto, HomeViewModel>()
                .ForMember(dest => dest.RecentTasks, opt => opt.MapFrom(src => src.RecentTasks))
                .ForMember(dest => dest.RecentProjects, opt => opt.MapFrom(src => src.RecentProjects));

            CreateMap<Project, RecentProjectDto>()
                .ForMember(dest => dest.TotalTasks, opt => opt.MapFrom(src => src.ProjectTasks.Count))
                .ForMember(dest => dest.CompletedTasks, opt => opt.MapFrom(src => src.ProjectTasks.Count(t => t.BoardId == 3)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.ProjectDetail))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.TeamName));

            CreateMap<ProjectTask, RecentTaskDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.AppointeeName, opt => opt.MapFrom(src => src.Appointee.User.Name));

            CreateMap<RecentTaskDto, RecentTaskViewModel>().ReverseMap();
            CreateMap<RecentProjectDto, RecentProjectViewModel>().ReverseMap();
        }
    }
}
