using ArslanProjectManager.Core;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
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
            CreateMap<UserUpdateDto, User>();

            //WEBSITE VIEW MODELS
            CreateMap<UserLoginDto, LoginViewModel>().ReverseMap();
        }
    }
}
