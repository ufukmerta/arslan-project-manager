using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Manages team operations including team creation, member management, and team invitations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController(ITeamService teamService) : CustomBaseController
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetByToken()
        {
            return CreateActionResult(await teamService.GetUserTeamsAsync(CurrentUserId));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        public async Task<IActionResult> Details(int id)
        {
            var validationResult = ValidateId(id, ErrorMessages.InvalidTeamId);
            if (validationResult != null)
            {
                return validationResult;
            }

            return CreateActionResult(await teamService.GetTeamDetailsAsync(CurrentUserId, id));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(TeamCreateDto model)
        {
            return CreateActionResult(await teamService.CreateTeamAsync(CurrentUserId, model));
        }

        [HttpGet("{id:int}/[action]-meta")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> Invite(int id)
        {
            return CreateActionResult(await teamService.GetInviteMetaAsync(CurrentUserId, id));
        }

        [HttpPost("{id:int}/invites")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> Invite(int id, TeamInviteCreateDto model)
        {
            var validationResult = ValidateId(id, ErrorMessages.InvalidTeamId);
            if (validationResult != null)
            {
                return validationResult;
            }

            return CreateActionResult(await teamService.InviteMemberAsync(CurrentUserId, id, model));
        }

        [HttpGet("{id:int}/[action]")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> Invites(int id)
        {
            var validationResult = ValidateId(id, ErrorMessages.InvalidTeamId);
            if (validationResult != null)
            {
                return validationResult;
            }

            return CreateActionResult(await teamService.GetTeamInvitesAsync(CurrentUserId, id));
        }

        [HttpDelete("/api/invites/{id:int}")]
        [ServiceFilter(typeof(NotFoundFilter<TeamInvite>))]
        [Authorize]
        public async Task<IActionResult> CancelInvite(int id)
        {
            var validationResult = ValidateId(id, ErrorMessages.InvalidInviteId);
            if (validationResult != null)
            {
                return validationResult;
            }

            return CreateActionResult(await teamService.CancelInviteAsync(CurrentUserId, id));
        }

        [HttpGet("{id:int}/permissions")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> GetPermissions(int id)
        {
            var validationResult = ValidateId(id, ErrorMessages.InvalidTeamId);
            if (validationResult != null)
            {
                return validationResult;
            }

            return CreateActionResult(await teamService.GetPermissionsAsync(CurrentUserId, id));
        }

        [HttpGet("{teamId:int}/roles")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> GetRoles(int teamId)
        {
            return CreateActionResult(await teamService.GetRolesAsync(CurrentUserId, teamId));
        }

        [HttpPost("{teamId:int}/roles")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> CreateRole(int teamId, RoleCreateDto model)
        {
            return CreateActionResult(await teamService.CreateRoleAsync(CurrentUserId, teamId, model));
        }

        [HttpPut("{teamId:int}/roles/{roleId:int}")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> UpdateRole(int teamId, int roleId, RoleUpdateDto model)
        {
            return CreateActionResult(await teamService.UpdateRoleAsync(CurrentUserId, teamId, roleId, model));
        }

        [HttpDelete("{teamId:int}/roles/{roleId:int}")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> DeleteRole(int teamId, int roleId)
        {
            return CreateActionResult(await teamService.DeleteRoleAsync(CurrentUserId, teamId, roleId));
        }

        [HttpPost("{teamId:int}/roles/clone")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> CloneRole(int teamId, RoleCloneDto model)
        {
            return CreateActionResult(await teamService.CloneRoleAsync(CurrentUserId, teamId, model));
        }

        [HttpPut("{teamId:int}/users/{userId:int}/permissions")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> UpdateUserPermissions(int teamId, int userId, UserPermissionUpdateDto model)
        {
            return CreateActionResult(await teamService.UpdateUserPermissionsAsync(CurrentUserId, teamId, userId, model));
        }

        [HttpGet("{teamId:int}/users/{userId:int}/permissions")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> GetUserPermissions(int teamId, int userId)
        {
            return CreateActionResult(await teamService.GetUserPermissionsAsync(CurrentUserId, teamId, userId));
        }
    }
}
