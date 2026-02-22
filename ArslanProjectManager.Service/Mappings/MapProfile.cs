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
            //Dto<->Model
            CreateMap<BoardTagDto, BoardTag>().ReverseMap();
            CreateMap<LogCategoryDto, LogCategory>().ReverseMap();
            CreateMap<ProjectDto, Project>().ReverseMap();
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamId == null ? null : src.Team!.TeamName))
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
                .ForMember(dest => dest.UserCount, opt => opt.Ignore()); // Will be set manually in service
            CreateMap<TaskCategoryDto, TaskCategory>().ReverseMap();

            CreateMap<TaskCommentDto, TaskComment>()
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

            //CreateDto<->Model
            CreateMap<BoardTag, BoardTagCreateDto>().ReverseMap();
            CreateMap<LogCategory, LogCategoryCreateDto>().ReverseMap();

            CreateMap<ProjectTaskCreateDto, ProjectTask>()
                .ForMember(dest => dest.AppointerId, opt => opt.Ignore());

            CreateMap<Project, ProjectCreateDto>().ReverseMap();
            CreateMap<Role, RoleCreateDto>();
            CreateMap<RoleCreateDto, Role>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TeamId, opt => opt.Ignore()) // Will be set in service
                .ForMember(dest => dest.IsSystemRole, opt => opt.MapFrom(src => false)) // Team-specific role, not system role
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
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
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore());

            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore());

            //UpdateDto<->Model
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
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
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
                .ForMember(dest => dest.RoleId, opt => opt.Ignore()) // Will be set if RoleId is provided in DTO
                .ForMember(dest => dest.CanViewTasksOverride, opt => opt.MapFrom(src => src.CanViewTasks))
                .ForMember(dest => dest.CanEditTasksOverride, opt => opt.MapFrom(src => src.CanEditTasks))
                .ForMember(dest => dest.CanDeleteTasksOverride, opt => opt.MapFrom(src => src.CanDeleteTasks))
                .ForMember(dest => dest.CanAssignTasksOverride, opt => opt.MapFrom(src => src.CanAssignTasks))
                .ForMember(dest => dest.CanViewProjectsOverride, opt => opt.MapFrom(src => src.CanViewProjects))
                .ForMember(dest => dest.CanEditProjectsOverride, opt => opt.MapFrom(src => src.CanEditProjects))
                .ForMember(dest => dest.CanDeleteProjectsOverride, opt => opt.MapFrom(src => src.CanDeleteProjects))
                .ForMember(dest => dest.CanInviteMembersOverride, opt => opt.MapFrom(src => src.CanInviteMembers))
                .ForMember(dest => dest.CanRemoveMembersOverride, opt => opt.MapFrom(src => src.CanRemoveMembers))
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
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore());

            CreateMap<User, UserUpdateDto>()
                .ForMember(dest => dest.ProfilePicture, src => src.Ignore());

            //DeleteDto<->Model
            CreateMap<Project, ProjectDeleteDto>()
            .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.TeamName))
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.ProjectTasks.Count))
            .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.ProjectTasks.Count(t => t.BoardId == 3)));
            CreateMap<ProjectTask, ProjectTaskDeleteDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
                .ForMember(dest => dest.BoardName, opt => opt.MapFrom(src => src.Board.BoardName))
                .ForMember(dest => dest.TaskCategoryName, opt => opt.MapFrom(src => src.TaskCategory.Category))
                .ForMember(dest => dest.AppointeeName, opt => opt.MapFrom(src => src.Appointee.User.Name))
                .ForMember(dest => dest.AppointerName, opt => opt.MapFrom(src => src.Appointer.User.Name));


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

            //User/MyInvites
            CreateMap<PendingInviteDto, PendingInviteViewModel>()
                .ForMember(dest => dest.TeamInviteId, opt => opt.MapFrom(src => src.TeamInviteId))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamName))
                .ForMember(dest => dest.InvitedByName, opt => opt.MapFrom(src => src.InvitedByName))
                .ForMember(dest => dest.InviteDate, opt => opt.MapFrom(src => src.InviteDate)).ReverseMap();

            CreateMap<TeamInvite, PendingInviteDto>()
                .ForMember(dest => dest.TeamInviteId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.TeamName))
                .ForMember(dest => dest.InvitedByName, opt => opt.MapFrom(src => src.InvitedBy.Name))
                .ForMember(dest => dest.InviteDate, opt => opt.MapFrom(src => src.CreatedDate));



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
                .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.CompletedTaskCount))
                .ForMember(dest => dest.CanEdit, opt => opt.MapFrom(src => src.CanEdit))
                .ForMember(dest => dest.CanDelete, opt => opt.MapFrom(src => src.CanDelete)).ReverseMap();

            CreateMap<Project, UserProjectDto>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.ProjectDetail))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.TeamName))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.Team.ManagerId))
                .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.ProjectTasks.Count))
                .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.ProjectTasks.Count(pt => pt.BoardId == 3)));


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
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
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


            //Teams
            //Teams/Index
            CreateMap<TeamDto, TeamViewModel>()
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.ManagerName))
                .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.MemberCount))
                .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.MemberCount))
                .ForMember(dest => dest.ProjectCount, opt => opt.MapFrom(src => src.ProjectCount)).ReverseMap();

            //Teams/Details
            CreateMap<TeamDetailsDto, TeamDetailsViewModel>()
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.ManagerName))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId))
                .ForMember(dest => dest.CanRemoveMembers, opt => opt.MapFrom(src => src.CanRemoveMembers))
                .ForMember(dest => dest.CanEditProjects, opt => opt.MapFrom(src => src.CanEditProjects))
                .ForMember(dest => dest.Projects, opt => opt.MapFrom(src => src.Projects))
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members)).ReverseMap();

            CreateMap<TeamUserDto, TeamMemberViewModel>()
                .ForMember(dest => dest.TeamUserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role)).ReverseMap();

            CreateMap<TeamProjectDto, ProjectViewModel>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.TaskCount))
                .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.CompletedTaskCount)).ReverseMap();

            //Teams/Create
            CreateMap<TeamCreateViewModel, TeamCreateDto>()
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ManagerId, opt => opt.Ignore()).ReverseMap();

            //Teams/Invite
            CreateMap<TeamInviteViewModel, TeamInviteCreateViewDto>()
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamId))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamName))
                .ForMember(dest => dest.InvitedById, opt => opt.Ignore())
                .ForMember(dest => dest.InviterName, opt => opt.MapFrom(src => src.InviterName)).ReverseMap();

            CreateMap<TeamInviteViewModel, TeamInviteCreateDto>()
                .ForMember(dest => dest.InvitedEmail, opt => opt.MapFrom(src => src.InvitedEmail)).ReverseMap();

            //Teams/Invites
            CreateMap<TeamInviteListDto, TeamInviteItemViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.InvitedEmail, opt => opt.MapFrom(src => src.InvitedEmail))
                .ForMember(dest => dest.InvitedByName, opt => opt.MapFrom(src => src.InvitedByName))
                .ForMember(dest => dest.InvitedById, opt => opt.MapFrom(src => src.InvitedById))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.StatusChangeNote, opt => opt.MapFrom(src => src.StatusChangeNote))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate)).ReverseMap();


            //Teams/Permissions
            CreateMap<TeamPermissionsDto, TeamPermissionsViewModel>()
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamId))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamName))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId))
                .ForMember(dest => dest.CanManagePermissions, opt => opt.MapFrom(src => src.CanManagePermissions))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.Users)).ReverseMap();

            CreateMap<TeamUserRoleDto, TeamUserPermissionViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.TeamUserId, opt => opt.MapFrom(src => src.TeamUserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsSystemRole, opt => opt.MapFrom(src => src.IsSystemRole))
                .ForMember(dest => dest.CanViewTasks, opt => opt.MapFrom(src => src.CanViewTasks))
                .ForMember(dest => dest.CanEditTasks, opt => opt.MapFrom(src => src.CanEditTasks))
                .ForMember(dest => dest.CanDeleteTasks, opt => opt.MapFrom(src => src.CanDeleteTasks))
                .ForMember(dest => dest.CanAssignTasks, opt => opt.MapFrom(src => src.CanAssignTasks))
                .ForMember(dest => dest.CanViewProjects, opt => opt.MapFrom(src => src.CanViewProjects))
                .ForMember(dest => dest.CanEditProjects, opt => opt.MapFrom(src => src.CanEditProjects))
                .ForMember(dest => dest.CanDeleteProjects, opt => opt.MapFrom(src => src.CanDeleteProjects))
                .ForMember(dest => dest.CanInviteMembers, opt => opt.MapFrom(src => src.CanInviteMembers))
                .ForMember(dest => dest.CanRemoveMembers, opt => opt.MapFrom(src => src.CanRemoveMembers))
                .ForMember(dest => dest.CanManageRoles, opt => opt.MapFrom(src => src.CanManageRoles))
                .ForMember(dest => dest.CanManagePermissions, opt => opt.MapFrom(src => src.CanManagePermissions))
                .ReverseMap();

            //Teams/Roles
            CreateMap<RoleDto, TeamRoleViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamId))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.TeamName))
                .ForMember(dest => dest.IsSystemRole, opt => opt.MapFrom(src => src.IsSystemRole))
                .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => src.UserCount))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions))
                .ReverseMap();

            CreateMap<RolePermissionsDto, RolePermissionsViewModel>().ReverseMap();

            CreateMap<TeamRoleCreateViewModel, RoleCreateDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions))
                .ReverseMap();

            CreateMap<TeamRoleUpdateViewModel, RoleUpdateDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions))
                .ReverseMap();

            //Teams/User Permissions
            CreateMap<UserEffectivePermissionsDto, UserEffectivePermissionsViewModel>().ReverseMap();
            CreateMap<UserPermissionUpdateViewModel, UserPermissionUpdateDto>().ReverseMap();
        }
    }
}
