using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Core.Services
{
    public interface ITeamService : IGenericService<Team>
    {
        Task<TeamUser?> GetTeamUserAsync(int teamId, int userId);
        Task<TeamUser> AddTeamUserAsync(TeamUser teamUser);
        Task<TeamPermissionsDto> GetUserTeamPermissionsAsync(int teamId, Token token);
        Task<CustomResponseDto<IEnumerable<TeamDto>>> GetUserTeamsAsync(int userId);
        Task<CustomResponseDto<TeamDetailsDto>> GetTeamDetailsAsync(int userId, int teamId);
        Task<CustomResponseDto<MiniTeamDto>> CreateTeamAsync(int userId, TeamCreateDto model);
        Task<CustomResponseDto<TeamInviteCreateViewDto>> GetInviteMetaAsync(int userId, int teamId);
        Task<CustomResponseDto<NoContentDto>> InviteMemberAsync(int userId, int teamId, TeamInviteCreateDto model);
        Task<CustomResponseDto<IEnumerable<TeamInviteListDto>>> GetTeamInvitesAsync(int userId, int teamId);
        Task<CustomResponseDto<NoContentDto>> CancelInviteAsync(int userId, int inviteId);
        Task<CustomResponseDto<TeamPermissionsDto>> GetPermissionsAsync(int userId, int teamId);
        Task<CustomResponseDto<IEnumerable<RoleDto>>> GetRolesAsync(int userId, int teamId);
        Task<CustomResponseDto<NoContentDto>> CreateRoleAsync(int userId, int teamId, RoleCreateDto model);
        Task<CustomResponseDto<NoContentDto>> UpdateRoleAsync(int userId, int teamId, int roleId, RoleUpdateDto model);
        Task<CustomResponseDto<NoContentDto>> DeleteRoleAsync(int userId, int teamId, int roleId);
        Task<CustomResponseDto<NoContentDto>> CloneRoleAsync(int userId, int teamId, RoleCloneDto model);
        Task<CustomResponseDto<NoContentDto>> UpdateUserPermissionsAsync(int currentUserId, int teamId, int targetUserId, UserPermissionUpdateDto model);
        Task<CustomResponseDto<UserEffectivePermissionsDto>> GetUserPermissionsAsync(int currentUserId, int teamId, int targetUserId);
    }
}
