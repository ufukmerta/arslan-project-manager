namespace ArslanProjectManager.Core.Constants
{
    public static class ErrorMessages
    {
        public const string Unauthorized = "Not logged in or access token is invalid";
        public const string AccessDenied = "Access denied";
        public const string ProjectNotFound = "Project not found";
        public const string NotTeamMember = "You're not a member of the team";
        public const string InvalidInput = "Invalid input";
        public const string NoPermission = "You do not have permission to perform this action";
        public const string ProcessingError = "An error occurred while processing your request. Please try again later";
        public const string DefaultRolesNotFound = "No default role found. Please contact administrator.";

        public const string InvalidProjectId = "Invalid project ID";
        public const string InvalidProjectData = "Invalid project data";
        public const string NoProjectsFound = "No projects found for this user";
        public const string NoTeamsFound = "No teams found for this user";
        public const string FailedToCreateProject = "Failed to create project";
        public const string NotAuthorizedToEditProject = "You're not authorized to edit this project";
        public const string TeamNotFound = "Team not found";
        public const string NoPermissionToCreateProject = "You do not have permission to create project";
        public const string NoPermissionToEditProject = "You do not have permission to edit project";
        public const string NoPermissionToDeleteProject = "You do not have permission to delete project";
        public const string NoPermissionToViewProject = "You do not have permission to view project";

        public const string TaskNotFound = "Task not found";
        public const string InvalidTaskData = "Invalid task data";
        public const string NoTasksFound = "No tasks found for you";
        public const string InvalidTaskId = "Invalid task ID";
        public const string FailedToCreateTask = "Failed to create task";
        public const string NoPermissionToCreateTask = "You do not have permission to create task in this project";
        public const string NoPermissionToEditTask = "You do not have permission to edit this task";
        public const string NoPermissionToDeleteTask = "You do not have permission to delete this task";
        public const string NoPermissionToViewTask = "You do not have permission to view this task";
        
        public const string InvalidTeamId = "Invalid team ID";
        public const string InvalidTeamData = "Invalid team data";
        public const string FailedToCreateTeam = "Failed to create team";
        public const string UserAlreadyTeamMember = "User is already a member of this team";
        public const string InvitationAlreadySent = "An invitation has already been sent to this email address";
        public const string FailedToInvite = "An error occurred while inviting";
        public const string InvitationNotFound = "Invitation not found";
        public const string InvalidInviteId = "Invalid invite ID";
        public const string OnlyPendingInvitationsCanBeCanceled = "Only pending invitations can be canceled";
        public const string NotAuthorizedToCancelInvitation = "You are not authorized to cancel this invitation";

        // User-specific error messages
        public const string TokenExpired = "Token is expired";
        public const string UserNotFound = "User not found";
        public const string InvalidCredentials = "Invalid email or password";
        public const string EmailAlreadyExists = "This email is already registered";
        public const string InvalidEmailFormat = "Invalid email format";
        public const string InvalidPasswordFormat = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*/?&+-_.)";
        public const string RefreshTokenMissing = "Refresh token is missing, invalid, or inactive";
        public const string RefreshTokenExpired = "Refresh token has expired";
        public const string TokenGenerationFailed = "Token generation failed";
        public const string CurrentPasswordIncorrect = "Current password is incorrect";
        public const string PasswordsRequired = "Current and new password are required to set a new password";
        public const string EmailAlreadyExistsForUpdate = "This email already exists. You cannot change your email with given email address";
        
        // Invite specific error messages
        public const string InviteNotFound = "Team invitation not found";
        public const string InviteAlreadyProcessed = "This invitation has already been processed";
        public const string FailedToAcceptInvite = "Failed to accept team invitation";
        public const string FailedToRejectInvite = "Failed to reject team invitation";
        public const string NoPendingInvites = "No pending team invitations found";

        // Role and Permission Management error messages
        public const string RoleNotFound = "Role not found";
        public const string InvalidRoleId = "Invalid role ID";
        public const string RoleNameAlreadyExists = "A role with this name already exists for this team";
        public const string SystemRoleCannotBeModified = "System roles cannot be modified or deleted";
        public const string RoleInUse = "This role is currently assigned to team members and cannot be deleted";
        public const string NotAuthorizedToManageRoles = "You are not authorized to manage roles for this team";
        public const string NotAuthorizedToManagePermissions = "You are not authorized to manage permissions for this team";
        public const string CannotChangeManagerPermissions = "Cannot change manager's own permissions";
        public const string FailedToCreateRole = "Failed to create role";
        public const string FailedToUpdateRole = "Failed to update role";
        public const string FailedToDeleteRole = "Failed to delete role";
        public const string FailedToUpdatePermissions = "Failed to update user permissions";
    }
}
