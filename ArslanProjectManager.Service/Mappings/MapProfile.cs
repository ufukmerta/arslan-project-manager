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

            CreateMap<BoardTag, BoardTagCreateDto>().ReverseMap();
            CreateMap<LogCategory, LogCategoryCreateDto>().ReverseMap();
            CreateMap<ProjectTask, ProjectTaskCreateDto>().ReverseMap();
            CreateMap<Role, RoleCreateDto>().ReverseMap();
            CreateMap<TaskCategory, TaskCategoryCreateDto>().ReverseMap();
            CreateMap<TaskComment, TaskCommentCreateDto>().ReverseMap();
            CreateMap<TaskLog, TaskLogCreateDto>().ReverseMap();
            CreateMap<TaskTag, TaskTagCreateDto>().ReverseMap();
            CreateMap<TeamInvite, TeamInviteCreateDto>().ReverseMap();
            CreateMap<Team, TeamCreateDto>().ReverseMap();
            CreateMap<TeamUser, TeamUserCreateDto>().ReverseMap();
            CreateMap<User, UserCreateDto>()
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore());
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore());

            CreateMap<BoardTagUpdateDto, BoardTag>().ReverseMap();
            CreateMap<LogCategoryUpdateDto, LogCategory>().ReverseMap();
            CreateMap<ProjectUpdateDto, Project>().ReverseMap();
            CreateMap<ProjectTaskUpdateDto, ProjectTask>().ReverseMap();
            CreateMap<RoleUpdateDto, Role>().ReverseMap();
            CreateMap<TaskCategoryUpdateDto, TaskCategory>().ReverseMap();
            CreateMap<TaskCommentUpdateDto, TaskComment>().ReverseMap();
            CreateMap<TaskLogUpdateDto, TaskLog>().ReverseMap();
            CreateMap<TaskTagUpdateDto, TaskTag>().ReverseMap();
            CreateMap<TeamUpdateDto, Team>().ReverseMap();
            CreateMap<TeamInviteUpdateDto, TeamInvite>().ReverseMap();
            CreateMap<TeamUserUpdateDto, TeamUser>().ReverseMap();
            CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore());
            CreateMap<User, UserUpdateDto>()
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore());

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

            //Projects
            //Projects/Index
            CreateMap<UserProjectDto, ProjectViewModel>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamName))
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamId))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId))
                .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.TaskCount))
                .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.CompletedTaskCount)).ReverseMap();

            //Projects/Details
            CreateMap<ProjectDetailsDto, ProjectDetailsViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
                .ForMember(dest => dest.ProjectDetail, opt => opt.MapFrom(src => src.ProjectDetail))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks)).ReverseMap();


            CreateMap<ProjectTaskDto, ProjectTaskViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.BoardId, opt => opt.MapFrom(src => src.BoardId))
                .ForMember(dest => dest.TaskCategoryId, opt => opt.MapFrom(src => src.TaskCategoryId))
                .ForMember(dest => dest.AppointeeId, opt => opt.MapFrom(src => src.AppointeeId))
                .ForMember(dest => dest.AppointerId, opt => opt.MapFrom(src => src.AppointerId))
                .ForMember(dest => dest.CanDelete, opt => opt.MapFrom(src => src.CanDelete))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId)).ReverseMap();

            CreateMap<ProjectTaskDto, MiniProjectTaskDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
                .ForMember(dest => dest.BoardId, opt => opt.MapFrom(src => src.BoardId))
                .ForMember(dest => dest.AppointeeId, opt => opt.MapFrom(src => src.AppointeeId))
                .ForMember(dest => dest.AppointeeName, opt => opt.MapFrom(src => src.AppointeeName))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority)).ReverseMap();

            CreateMap<MiniProjectTaskDto, ProjectTaskViewModel>().ReverseMap();

            CreateMap<TaskCommentDto, TaskCommentViewModel>()
                    .ForMember(dest => dest.CommentId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId))
                    .ForMember(dest => dest.TeamUserId, opt => opt.MapFrom(src => src.TeamUserId))
                    .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
                    .ForMember(dest => dest.CommenterName, opt => opt.MapFrom(src => src.CommenterName))
                    .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.CreatedDate))
                    .ReverseMap();

            CreateMap<TaskCommentCreateDto, CreateTaskCommentViewModel>().ReverseMap();

            //Projects/Create
            CreateMap<ProjectCreateDto, CreateProjectViewModel>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.ProjectDetail))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamId)).ReverseMap();

            CreateMap<MiniProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
                .ForMember(dest => dest.ProjectDetail, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.TeamId, opt => opt.Ignore())
                .ForMember(dest => dest.Team, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTasks, opt => opt.Ignore()).ReverseMap();

            //Projects/Edit
            CreateMap<EditProjectViewModel, ProjectUpdateDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
                .ForMember(dest => dest.ProjectDetail, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate)).ReverseMap();

            //Projects/Delete
            CreateMap<DeleteProjectViewModel, ProjectDeleteDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
                .ForMember(dest => dest.ProjectDetail, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamName))
                .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.TaskCount))
                .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.CompletedTaskCount)).ReverseMap();


            //Tasks
            //Tasks/Index
            CreateMap<ProjectTask, ProjectTaskViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.ExpectedEndDate, opt => opt.MapFrom(src => src.ExpectedEndDate))
                .ForMember(dest => dest.BoardId, opt => opt.MapFrom(src => src.BoardId))
                .ForMember(dest => dest.BoardName, opt => opt.MapFrom(src => src.Board.BoardName))
                .ForMember(dest => dest.TaskCategoryId, opt => opt.MapFrom(src => src.TaskCategoryId))
                .ForMember(dest => dest.TaskCategoryName, opt => opt.MapFrom(src => src.TaskCategory.Category))
                .ForMember(dest => dest.AppointeeId, opt => opt.MapFrom(src => src.AppointeeId))
                .ForMember(dest => dest.AppointeeName, opt => opt.MapFrom(src => src.Appointee.User.Name))
                .ForMember(dest => dest.AppointerId, opt => opt.MapFrom(src => src.AppointerId))
                .ForMember(dest => dest.AppointerName, opt => opt.MapFrom(src => src.Appointer.User.Name))
                .ForMember(dest => dest.CanDelete, opt => opt.Ignore())
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName)).ReverseMap();

            //Tasks/Details
            CreateMap<ProjectTaskDto, ProjectTaskViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.ExpectedEndDate, opt => opt.MapFrom(src => src.ExpectedEndDate))
                .ForMember(dest => dest.BoardId, opt => opt.MapFrom(src => src.BoardId))
                .ForMember(dest => dest.BoardName, opt => opt.MapFrom(src => src.BoardName))
                .ForMember(dest => dest.TaskCategoryId, opt => opt.MapFrom(src => src.TaskCategoryId))
                .ForMember(dest => dest.TaskCategoryName, opt => opt.MapFrom(src => src.TaskCategoryName))
                .ForMember(dest => dest.AppointeeId, opt => opt.MapFrom(src => src.AppointeeId))
                .ForMember(dest => dest.AppointeeName, opt => opt.MapFrom(src => src.AppointeeName))
                .ForMember(dest => dest.AppointerId, opt => opt.MapFrom(src => src.AppointerId))
                .ForMember(dest => dest.AppointerName, opt => opt.MapFrom(src => src.AppointerName))
                .ForMember(dest => dest.CanDelete, opt => opt.MapFrom(src => src.CanDelete))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName)).ReverseMap();

            //Tasks/Comment
            CreateMap<CreateTaskCommentViewModel, TaskCommentCreateDto>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId))
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment)).ReverseMap();

            //Tasks/Create
            CreateMap<CreateTaskViewModel, ProjectTaskCreateDto>()
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.ExpectedEndDate, opt => opt.MapFrom(src => src.ExpectedEndDate))
                .ForMember(dest => dest.BoardId, opt => opt.MapFrom(src => src.BoardId))
                .ForMember(dest => dest.TaskCategoryId, opt => opt.MapFrom(src => src.TaskCategoryId))
                .ForMember(dest => dest.AppointeeId, opt => opt.MapFrom(src => src.AppointeeId))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId)).ReverseMap();

            CreateMap<TaskUserDto, TaskUserViewModel>()
                .ForMember(dest => dest.TeamUserId, opt => opt.MapFrom(src => src.TeamUserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name)).ReverseMap();

            CreateMap<ProjectTaskCreateViewDto, CreateTaskViewViewModel>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.TeamMembers, opt => opt.MapFrom(src => src.TeamMembers))
                .ForMember(dest => dest.BoardTags, opt => opt.MapFrom(src => src.BoardTags))
                .ForMember(dest => dest.TaskCategories, opt => opt.MapFrom(src => src.TaskCategories)).ReverseMap();

            CreateMap<TaskUserViewModel, TaskUserDto>()
                .ForMember(dest => dest.TeamUserId, opt => opt.MapFrom(src => src.TeamUserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name)).ReverseMap();

            CreateMap<BoardTagViewModel, BoardTagDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BoardId))
                .ForMember(dest => dest.BoardName, opt => opt.MapFrom(src => src.BoardName))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BoardId)).ReverseMap();

            CreateMap<TaskCategoryViewModel, TaskCategoryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TaskCategoryId))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category)).ReverseMap();

            //Tasks/Edit
            CreateMap<ProjectTaskUpdateDto, EditTaskViewModel>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.ExpectedEndDate, opt => opt.MapFrom(src => src.ExpectedEndDate))
                .ForMember(dest => dest.BoardId, opt => opt.MapFrom(src => src.BoardId))
                .ForMember(dest => dest.TaskCategoryId, opt => opt.MapFrom(src => src.TaskCategoryId))
                .ForMember(dest => dest.AppointeeId, opt => opt.MapFrom(src => src.AppointeeId))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority)).ReverseMap();

            //Tasks/Delete
            CreateMap<ProjectTaskDeleteDto, DeleteTaskViewModel>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.BoardName, opt => opt.MapFrom(src => src.BoardName))
                .ForMember(dest => dest.TaskCategoryName, opt => opt.MapFrom(src => src.TaskCategoryName))
                .ForMember(dest => dest.AppointeeName, opt => opt.MapFrom(src => src.AppointeeName))
                .ForMember(dest => dest.AppointerName, opt => opt.MapFrom(src => src.AppointerName))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority)).ReverseMap();
        }
    }
}
