using ArslanProjectManager.Core.Constants;
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
            // Dto <-> Model
            CreateMap<BoardTagDto, BoardTag>().ReverseMap();
            CreateMap<LogCategoryDto, LogCategory>().ReverseMap();
            CreateMap<ProjectDto, Project>().ReverseMap();

            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team != null ? src.Team.TeamName : null))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => new RolePermissionsDto
                {
                    CanViewTasks = src.CanViewTasks,
                    CanEditTasks = src.CanEditTasks,
                    CanDeleteTasks = src.CanDeleteTasks,
                    CanAssignTasks = src.CanAssignTasks,
                    CanViewProjects = src.CanViewProjects,
                    CanEditProjects = src.CanEditProjects,
                    CanDeleteProjects = src.CanDeleteProjects,
                    CanInviteMembers = src.CanInviteMembers,
                    CanRemoveMembers = src.CanRemoveMembers,
                    CanManageRoles = src.CanManageRoles,
                    CanManagePermissions = src.CanManagePermissions
                }))
                .ForMember(dest => dest.UserCount, opt => opt.Ignore());

            CreateMap<TaskCategoryDto, TaskCategory>().ReverseMap();

            CreateMap<TaskCommentDto, TaskComment>()
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.TeamUser, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.CommenterName, opt => opt.MapFrom(src => src.TeamUser.User.Name));

            CreateMap<TaskLogDto, TaskLog>().ReverseMap();
            CreateMap<TaskTagDto, TaskTag>().ReverseMap();
            CreateMap<TeamDto, Team>().ReverseMap();
            CreateMap<TeamInviteDto, TeamInvite>().ReverseMap();
            CreateMap<TeamUserDto, TeamUser>().ReverseMap();
            CreateMap<UserDto, User>().ReverseMap();
            CreateMap<User, MiniUserDto>().ReverseMap();
            CreateMap<TokenDto, Token>().ReverseMap();

            CreateMap<ProjectTask, ProjectTaskDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.BoardName, opt => opt.MapFrom(src => src.Board.BoardName))
                .ForMember(dest => dest.TaskCategoryName, opt => opt.MapFrom(src => src.TaskCategory.Category))
                .ForMember(dest => dest.AppointerName, opt => opt.MapFrom(src => src.Appointer.User.Name))
                .ForMember(dest => dest.AppointeeName, opt => opt.MapFrom(src => src.Appointee.User.Name))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.TaskComments))
                .ForMember(dest => dest.CanEdit, opt => opt.Ignore())
                .ForMember(dest => dest.CanDelete, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Board, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Appointer, opt => opt.Ignore())
                .ForMember(dest => dest.Appointee, opt => opt.Ignore())
                .ForMember(dest => dest.TaskComments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskLogs, opt => opt.Ignore())
                .ForMember(dest => dest.TaskTags, opt => opt.Ignore());

            // CreateDto <-> Model
            CreateMap<BoardTag, BoardTagCreateDto>().ReverseMap();
            CreateMap<LogCategory, LogCategoryCreateDto>().ReverseMap();

            CreateMap<ProjectTaskCreateDto, ProjectTask>()
                .ForMember(dest => dest.AppointerId, opt => opt.Ignore());

            CreateMap<Project, ProjectCreateDto>().ReverseMap();
            CreateMap<Role, RoleCreateDto>();
            CreateMap<RoleCreateDto, Role>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TeamId, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemRole, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.CanViewTasks, opt => opt.MapFrom(src => src.Permissions.CanViewTasks))
                .ForMember(dest => dest.CanEditTasks, opt => opt.MapFrom(src => src.Permissions.CanEditTasks))
                .ForMember(dest => dest.CanDeleteTasks, opt => opt.MapFrom(src => src.Permissions.CanDeleteTasks))
                .ForMember(dest => dest.CanAssignTasks, opt => opt.MapFrom(src => src.Permissions.CanAssignTasks))
                .ForMember(dest => dest.CanViewProjects, opt => opt.MapFrom(src => src.Permissions.CanViewProjects))
                .ForMember(dest => dest.CanEditProjects, opt => opt.MapFrom(src => src.Permissions.CanEditProjects))
                .ForMember(dest => dest.CanDeleteProjects, opt => opt.MapFrom(src => src.Permissions.CanDeleteProjects))
                .ForMember(dest => dest.CanInviteMembers, opt => opt.MapFrom(src => src.Permissions.CanInviteMembers))
                .ForMember(dest => dest.CanRemoveMembers, opt => opt.MapFrom(src => src.Permissions.CanRemoveMembers))
                .ForMember(dest => dest.CanManageRoles, opt => opt.MapFrom(src => src.Permissions.CanManageRoles))
                .ForMember(dest => dest.CanManagePermissions, opt => opt.MapFrom(src => src.Permissions.CanManagePermissions))
                .ForMember(dest => dest.Team, opt => opt.Ignore())
                .ForMember(dest => dest.TeamUsers, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<TaskCategory, TaskCategoryCreateDto>().ReverseMap();
            CreateMap<TaskComment, TaskCommentCreateDto>().ReverseMap();
            CreateMap<TaskLog, TaskLogCreateDto>().ReverseMap();
            CreateMap<TaskTag, TaskTagCreateDto>().ReverseMap();
            CreateMap<TeamInvite, TeamInviteCreateDto>().ReverseMap();
            CreateMap<Team, TeamCreateDto>().ReverseMap();
            CreateMap<TeamUser, TeamUserCreateDto>().ReverseMap();

            CreateMap<User, UserCreateDto>()
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore());

            // UpdateDto <-> Model
            CreateMap<BoardTagUpdateDto, BoardTag>().ReverseMap();
            CreateMap<LogCategoryUpdateDto, LogCategory>().ReverseMap();
            CreateMap<ProjectUpdateDto, Project>().ReverseMap();

            CreateMap<ProjectTask, ProjectTaskUpdateDto>()
                .ForMember(dest => dest.TeamMembers, opt => opt.Ignore())
                .ForMember(dest => dest.BoardTags, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCategories, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.AppointerId, opt => opt.Ignore())
                .ForMember(dest => dest.Appointee, opt => opt.Ignore())
                .ForMember(dest => dest.Appointer, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Board, opt => opt.Ignore())
                .ForMember(dest => dest.TaskCategory, opt => opt.Ignore())
                .ForMember(dest => dest.TaskComments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskLogs, opt => opt.Ignore())
                .ForMember(dest => dest.TaskTags, opt => opt.Ignore());

            CreateMap<RoleUpdateDto, Role>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TeamId, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemRole, opt => opt.Ignore())
                .ForMember(dest => dest.CanViewTasks, opt => opt.MapFrom(src => src.Permissions.CanViewTasks))
                .ForMember(dest => dest.CanEditTasks, opt => opt.MapFrom(src => src.Permissions.CanEditTasks))
                .ForMember(dest => dest.CanDeleteTasks, opt => opt.MapFrom(src => src.Permissions.CanDeleteTasks))
                .ForMember(dest => dest.CanAssignTasks, opt => opt.MapFrom(src => src.Permissions.CanAssignTasks))
                .ForMember(dest => dest.CanViewProjects, opt => opt.MapFrom(src => src.Permissions.CanViewProjects))
                .ForMember(dest => dest.CanEditProjects, opt => opt.MapFrom(src => src.Permissions.CanEditProjects))
                .ForMember(dest => dest.CanDeleteProjects, opt => opt.MapFrom(src => src.Permissions.CanDeleteProjects))
                .ForMember(dest => dest.CanInviteMembers, opt => opt.MapFrom(src => src.Permissions.CanInviteMembers))
                .ForMember(dest => dest.CanRemoveMembers, opt => opt.MapFrom(src => src.Permissions.CanRemoveMembers))
                .ForMember(dest => dest.CanManageRoles, opt => opt.MapFrom(src => src.Permissions.CanManageRoles))
                .ForMember(dest => dest.CanManagePermissions, opt => opt.MapFrom(src => src.Permissions.CanManagePermissions))
                .ForMember(dest => dest.Team, opt => opt.Ignore())
                .ForMember(dest => dest.TeamUsers, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<TaskCategoryUpdateDto, TaskCategory>().ReverseMap();
            CreateMap<TaskCommentUpdateDto, TaskComment>().ReverseMap();
            CreateMap<TaskLogUpdateDto, TaskLog>().ReverseMap();
            CreateMap<TaskTagUpdateDto, TaskTag>().ReverseMap();
            CreateMap<TeamUpdateDto, Team>().ReverseMap();
            CreateMap<TeamInviteUpdateDto, TeamInvite>().ReverseMap();
            CreateMap<TeamUserUpdateDto, TeamUser>().ReverseMap();

            CreateMap<UserPermissionUpdateDto, TeamUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TeamId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForMember(dest => dest.CanViewTasksOverride, opt => opt.MapFrom(src => src.CanViewTasks))
                .ForMember(dest => dest.CanEditTasksOverride, opt => opt.MapFrom(src => src.CanEditTasks))
                .ForMember(dest => dest.CanDeleteTasksOverride, opt => opt.MapFrom(src => src.CanDeleteTasks))
                .ForMember(dest => dest.CanAssignTasksOverride, opt => opt.MapFrom(src => src.CanAssignTasks))
                .ForMember(dest => dest.CanViewProjectsOverride, opt => opt.MapFrom(src => src.CanViewProjects))
                .ForMember(dest => dest.CanEditProjectsOverride, opt => opt.MapFrom(src => src.CanEditProjects))
                .ForMember(dest => dest.CanDeleteProjectsOverride, opt => opt.MapFrom(src => src.CanDeleteProjects))
                .ForMember(dest => dest.CanInviteMembersOverride, opt => opt.MapFrom(src => src.CanInviteMembers))
                .ForMember(dest => dest.CanRemoveMembersOverride, opt => opt.MapFrom(src => src.CanRemoveMembers))
                .ForMember(dest => dest.CanManageRolesOverride, opt => opt.MapFrom(src => src.CanManageRoles))
                .ForMember(dest => dest.CanManagePermissionsOverride, opt => opt.MapFrom(src => src.CanManagePermissions))
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Team, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTaskAppointees, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTaskAppointers, opt => opt.Ignore())
                .ForMember(dest => dest.TaskComments, opt => opt.Ignore())
                .ForMember(dest => dest.TaskLogs, opt => opt.Ignore())
                .ForMember(dest => dest.AffectedTaskLogs, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore());

            // DeleteDto <-> Model
            CreateMap<Project, ProjectDeleteDto>()
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.TeamName))
                .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.ProjectTasks.Count))
                .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.ProjectTasks.Count(t => t.BoardId == SystemBoards.DoneId)));

            CreateMap<ProjectTask, ProjectTaskDeleteDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.BoardName, opt => opt.MapFrom(src => src.Board.BoardName))
                .ForMember(dest => dest.TaskCategoryName, opt => opt.MapFrom(src => src.TaskCategory.Category))
                .ForMember(dest => dest.AppointeeName, opt => opt.MapFrom(src => src.Appointee.User.Name))
                .ForMember(dest => dest.AppointerName, opt => opt.MapFrom(src => src.Appointer.User.Name));

            // WEBSITE & MOBILE VIEW MODELS
            // User
            CreateMap<UserProfileDto, UserProfileViewModel>()
                .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => src.ProfilePicture))
                .ReverseMap();

            CreateMap<UserLoginDto, LoginViewModel>().ReverseMap();

            CreateMap<UserCreateDto, RegisterViewModel>()
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<UserUpdateDto, EditUserViewModel>()
                .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => src.ProfilePicture))
                .ForMember(dest => dest.ProfilePictureFile, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<UserPasswordUpdateDto, ChangePasswordViewModel>()
                .ForMember(dest => dest.ConfirmNewPassword, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<PendingInviteDto, PendingInviteViewModel>().ReverseMap();

            CreateMap<TeamInvite, PendingInviteDto>()
                .ForMember(dest => dest.TeamInviteId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.TeamName))
                .ForMember(dest => dest.InvitedByName, opt => opt.MapFrom(src => src.InvitedBy.Name))
                .ForMember(dest => dest.InviteDate, opt => opt.MapFrom(src => src.CreatedDate));

            // Home
            CreateMap<HomeDto, HomeViewModel>();

            CreateMap<Project, RecentProjectDto>()
                .ForMember(dest => dest.TotalTasks, opt => opt.MapFrom(src => src.ProjectTasks.Count))
                .ForMember(dest => dest.CompletedTasks, opt => opt.MapFrom(src => src.ProjectTasks.Count(t => t.BoardId == SystemBoards.DoneId)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.ProjectDetail))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.TeamName));

            CreateMap<ProjectTask, RecentTaskDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.AppointeeName, opt => opt.MapFrom(src => src.Appointee.User.Name));

            CreateMap<RecentTaskDto, RecentTaskViewModel>().ReverseMap();
            CreateMap<RecentProjectDto, RecentProjectViewModel>().ReverseMap();

            // Projects
            CreateMap<UserProjectDto, ProjectViewModel>().ReverseMap();

            CreateMap<Project, UserProjectDto>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.ProjectDetail))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.TeamName))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.Team.ManagerId))
                .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.ProjectTasks.Count))
                .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.ProjectTasks.Count(pt => pt.BoardId == SystemBoards.DoneId)));

            CreateMap<ProjectDetailsDto, ProjectDetailsViewModel>().ReverseMap();
            CreateMap<ProjectTaskDto, MiniProjectTaskDto>().ReverseMap();
            CreateMap<MiniProjectTaskDto, ProjectTaskViewModel>().ReverseMap();

            CreateMap<TaskCommentDto, TaskCommentViewModel>()
                .ForMember(dest => dest.CommentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.CreatedDate))
                .ReverseMap();

            CreateMap<TaskCommentCreateDto, CreateTaskCommentViewModel>().ReverseMap();

            CreateMap<ProjectCreateDto, CreateProjectViewModel>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.ProjectDetail))
                .ReverseMap();

            CreateMap<MiniProjectDto, Project>()
                .ForMember(dest => dest.ProjectDetail, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.TeamId, opt => opt.Ignore())
                .ForMember(dest => dest.Team, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTasks, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<EditProjectViewModel, ProjectUpdateDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.ProjectDetail, opt => opt.MapFrom(src => src.Description))
                .ReverseMap();

            CreateMap<DeleteProjectViewModel, ProjectDeleteDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.ProjectDetail, opt => opt.MapFrom(src => src.Description))
                .ReverseMap();

            // Tasks
            CreateMap<ProjectTask, ProjectTaskViewModel>()
                .ForMember(dest => dest.BoardName, opt => opt.MapFrom(src => src.Board.BoardName))
                .ForMember(dest => dest.TaskCategoryName, opt => opt.MapFrom(src => src.TaskCategory.Category))
                .ForMember(dest => dest.AppointeeName, opt => opt.MapFrom(src => src.Appointee.User.Name))
                .ForMember(dest => dest.AppointerName, opt => opt.MapFrom(src => src.Appointer.User.Name))
                .ForMember(dest => dest.CanDelete, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ReverseMap();

            CreateMap<ProjectTaskDto, ProjectTaskViewModel>().ReverseMap();
            CreateMap<CreateTaskViewModel, ProjectTaskCreateDto>().ReverseMap();
            CreateMap<TaskUserDto, TaskUserViewModel>().ReverseMap();
            CreateMap<ProjectTaskCreateViewDto, CreateTaskViewViewModel>().ReverseMap();

            CreateMap<BoardTagViewModel, BoardTagDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BoardId))
                .ReverseMap();

            CreateMap<TaskCategoryViewModel, TaskCategoryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TaskCategoryId))
                .ReverseMap();

            CreateMap<ProjectTaskUpdateDto, EditTaskViewModel>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<ProjectTaskDeleteDto, DeleteTaskViewModel>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            // Teams
            CreateMap<TeamDto, TeamViewModel>()
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<TeamDetailsDto, TeamDetailsViewModel>()
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<TeamUserDto, TeamMemberViewModel>()
                .ForMember(dest => dest.TeamUserId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<TeamProjectDto, ProjectViewModel>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<TeamCreateViewModel, TeamCreateDto>()
                .ForMember(dest => dest.ManagerId, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<TeamInviteViewModel, TeamInviteCreateViewDto>()
                .ForMember(dest => dest.InvitedById, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<TeamInviteViewModel, TeamInviteCreateDto>().ReverseMap();
            CreateMap<TeamInviteListDto, TeamInviteItemViewModel>().ReverseMap();
            CreateMap<TeamPermissionsDto, TeamPermissionsViewModel>().ReverseMap();
            CreateMap<TeamUserRoleDto, TeamUserPermissionViewModel>().ReverseMap();
            CreateMap<RoleDto, TeamRoleViewModel>().ReverseMap();
            CreateMap<RolePermissionsDto, RolePermissionsViewModel>().ReverseMap();
            CreateMap<TeamRoleCreateViewModel, RoleCreateDto>().ReverseMap();
            CreateMap<TeamRoleUpdateViewModel, RoleUpdateDto>().ReverseMap();
            CreateMap<UserEffectivePermissionsDto, UserEffectivePermissionsViewModel>().ReverseMap();
            CreateMap<UserPermissionUpdateViewModel, UserPermissionUpdateDto>().ReverseMap();
        }
    }
}
