using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ArslanProjectManager.Core.Models.TeamInvite;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Manages team operations including team creation, member management, and team invitations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController(ProjectManagerDbContext context, ITokenService tokenService, ITeamService teamService, ITeamInviteService teamInviteService, IUserService userService) : CustomBaseController(tokenService)
    {
        private readonly ProjectManagerDbContext _context = context;
        private readonly ITeamService _teamService = teamService;
        private readonly ITeamInviteService _teamInviteService = teamInviteService;
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Retrieves all teams for the authenticated user (as manager or member)
        /// </summary>
        /// <returns>List of teams that the user has access to</returns>
        /// <response code="200">Returns the list of user's teams</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If no teams are found for the user</response>
        [HttpGet()]
        [Authorize]
        public async Task<IActionResult> GetByToken()
        {
            var token = (await GetToken())!;
            var doesTeamExist = await _teamService.AnyAsync(x => x.TeamUsers.Any(x => x.UserId == token.UserId));
            if (!doesTeamExist)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            var teams = await _context.Teams
                .Include(x => x.TeamUsers)
                .Include(x => x.Manager)
                .Include(x => x.Projects)
                .Where(x => x.Manager.Id == token.UserId ||
                           x.TeamUsers.Any(x => x.UserId == token.UserId))
                .OrderBy(x => x.Id)
                .Select(x => new TeamDto
                {
                    Id = x.Id,
                    TeamName = x.TeamName,
                    Description = x.TeamName,
                    ManagerName = x.Manager.Name,
                    MemberCount = x.TeamUsers.Count,
                    ProjectCount = x.Projects.Count
                })
                .ToListAsync();

            return CreateActionResult(CustomResponseDto<IEnumerable<TeamDto>>.Success(teams, 200));
        }

        /// <summary>
        /// Retrieves detailed information about a specific team
        /// </summary>
        /// <param name="id">The unique identifier of the team</param>
        /// <returns>Detailed team information including members and projects</returns>
        /// <response code="200">Returns the team details</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have access to this team</response>
        /// <response code="404">If the team is not found</response>
        [HttpGet("{id:int}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        public async Task<IActionResult> Details(int id)
        {
            var token = (await GetToken())!;

            var validationResult = ValidateModel(id, x => x > 0, ErrorMessages.InvalidTeamId);
            if (validationResult != null) return validationResult;            

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var teamDetailsDto = await _context.Teams
            .Include(x => x.Manager)
            .Include(x => x.TeamUsers)
            .ThenInclude(x => x.User)
            .Include(x => x.Projects)
            .ThenInclude(x => x.ProjectTasks)
            .Where(x => x.Id == id && x.TeamUsers.Any(tu => tu.UserId == token.UserId))
            .Select(x => new TeamDetailsDto
            {
                Id = x.Id,
                TeamName = x.TeamName,
                Description = x.TeamName,
                ManagerId = x.ManagerId,
                ManagerName = x.Manager.Name,
                Members = x.TeamUsers.Select(u => new TeamUserDto
                {
                    UserId = u.UserId,
                    Name = u.User.Name,
                    Email = u.User.Email,
                    Role = u.RoleId == 1 ? "Admin" : "Member"
                }).ToList(),
                Projects = x.Projects.Select(p => new TeamProjectDto
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.ProjectDetail,
                    TaskCount = p.ProjectTasks.Count,
                    CompletedTaskCount = p.ProjectTasks.Count(t => t.BoardId == 3)
                }).ToList()
            })
            .FirstOrDefaultAsync();
            if (teamDetailsDto is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            return CreateActionResult(CustomResponseDto<TeamDetailsDto>.Success(teamDetailsDto, 200));
        }

        /// <summary>
        /// Creates a new team
        /// </summary>
        /// <param name="model">The team creation details</param>
        /// <returns>The ID of the created team</returns>
        /// <response code="200">Returns the ID of the created team</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If the team creation fails</response>
        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> Create(TeamCreateDto model)
        {
            var token = (await GetToken())!;

            var team = new Team
            {
                TeamName = model.TeamName,
                Description = model.Description,
                ManagerId = token.UserId
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                _context.TeamUsers.Add(new TeamUser
                {
                    UserId = token.UserId,
                    RoleId = 1,
                    TeamId = team.Id
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreateActionResult(CustomResponseDto<MiniTeamDto>.Success(new MiniTeamDto { Id = team.Id }, 200));
            }
            catch
            {
                await transaction.RollbackAsync();
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToCreateTeam));
            }
        }

        /// <summary>
        /// Retrieves invitation metadata for a specific team
        /// </summary>
        /// <param name="id">The unique identifier of the team</param>
        /// <returns>Team invitation metadata including team name and inviter information</returns>
        /// <response code="200">Returns the invitation metadata</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the team is not found</response>
        [HttpGet("{id:int}/[action]-meta")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> Invite(int id)
        {
            var token = (await GetToken())!;
            var user = await _userService.GetByIdAsync(token.UserId);

            var team = await _context.Teams
               .Include(x => x.Manager)
               .Include(x => x.TeamUsers)
               .ThenInclude(x => x.User)
               .FirstOrDefaultAsync(x => x.Id == id);

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var teamInviteCreateViewDto = new TeamInviteCreateViewDto
            {
                TeamId = team!.Id,
                TeamName = team.TeamName,
                InviterName = user.Name,
            };

            return CreateActionResult(CustomResponseDto<TeamInviteCreateViewDto>.Success(teamInviteCreateViewDto, 200));
        }

        /// <summary>
        /// Sends an invitation to join a team
        /// </summary>
        /// <param name="id">The unique identifier of the team</param>
        /// <param name="model">The invitation details</param>
        /// <returns>Success or failure</returns>
        /// <response code="201">Invitation sent successfully</response>
        /// <response code="400">If the invitation already exists or user is already a member</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the team is not found</response>
        /// <response code="500">If invitation sending fails</response>
        [HttpPost("{id:int}/invites")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> Invite(int id, TeamInviteCreateDto model)
        {
            var token = (await GetToken())!;

            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidTeamId));
            }

            var team = await _context.Teams
                .Include(x => x.TeamUsers)
                .ThenInclude(x => x.User)
                .Include(x => x.TeamInvites)
                .FirstOrDefaultAsync(x => x.Id == id);

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var invitedUser = await _userService.GetByEmailAsync(model.InvitedEmail);

            if (invitedUser != null && team!.TeamUsers.Any(x => x.UserId == invitedUser.Id))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.UserAlreadyTeamMember));
            }

            if (team!.TeamInvites.Any(x => x.InvitedEmail == model.InvitedEmail && x.Status == InviteStatus.Pending))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvitationAlreadySent));
            }

            var teamInvite = new TeamInvite
            {
                TeamId = id,
                InvitedEmail = model.InvitedEmail,
                InvitedById = token.UserId,
                Status = InviteStatus.Pending,
            };

            var createdInvite = await _teamInviteService.AddAsync(teamInvite);
            if (createdInvite.Id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.FailedToInvite));
            }
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(201));
        }

        /// <summary>
        /// Retrieves all invitations for a specific team
        /// </summary>
        /// <param name="id">The unique identifier of the team</param>
        /// <returns>List of invitations</returns>
        /// <response code="200">Returns the list of invitations</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the team is not found</response>
        [HttpGet("{id:int}/[action]")]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        [Authorize]
        public async Task<IActionResult> Invites(int id)
        {
            var token = (await GetToken())!;

            var validationResult = ValidateModel(id, x => x > 0, ErrorMessages.InvalidTeamId);
            if (validationResult != null) return validationResult;

            var team = await _context.Teams
                .Include(x => x.TeamUsers)
                .Include(x => x.TeamInvites)
                .ThenInclude(x => x.InvitedBy)
                .FirstOrDefaultAsync(x => x.Id == id);

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var teamInvites = team!.TeamInvites
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new TeamInviteListDto
                {
                    Id = x.Id,
                    TeamId = x.TeamId,
                    TeamName = team.TeamName,
                    ManagerId = team.ManagerId,
                    InvitedEmail = x.InvitedEmail,
                    InvitedById = x.InvitedById,
                    InvitedByName = x.InvitedBy.Name,
                    Status = x.Status,
                    StatusChangeNote = x.StatusChangeNote,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToList();

            return CreateActionResult(CustomResponseDto<IEnumerable<TeamInviteListDto>>.Success(teamInvites, 200));
        }

        /// <summary>
        /// Cancels an invitation
        /// </summary>
        /// <param name="teamId">The unique identifier of the team</param>
        /// <param name="id">The invitation ID to cancel</param>
        /// <returns>Success or failure</returns>
        /// <response code="200">Invitation cancelled successfully</response>
        /// <response code="400">If the invitation ID is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to cancel the invitation</response>
        /// <response code="404">If the invitation is not found</response>
        [HttpDelete("/api/invites/{id:int}")]
        [ServiceFilter(typeof(NotFoundFilter<TeamInvite>))]
        [Authorize]
        public async Task<IActionResult> CancelInvite(int id)
        {
            var token = (await GetToken())!;

            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidInviteId));
            }

            var teamInvite = await _context.TeamInvites
                .Include(x => x.Team)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (teamInvite!.Status != InviteStatus.Pending)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.OnlyPendingInvitationsCanBeCanceled));
            }

            var isAuthorized = teamInvite.Team.ManagerId == token.UserId ||
                              teamInvite.InvitedById == token.UserId;

            if (!isAuthorized)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotAuthorizedToCancelInvitation));
            }

            teamInvite.Status = InviteStatus.Rejected;
            teamInvite.StatusChangeNote = "Canceled by " + (teamInvite.InvitedById == token.UserId ? "inviter" : "team manager");
            _teamInviteService.Update(teamInvite);
            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(200));
        }

        protected async Task<IActionResult?> ValidateTeamAccess(int teamId, int userId)
        {
            var token = (await GetToken())!;
            if (token.UserId != userId)
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.AccessDenied));

            var team = await _context.Teams
                .Include(t => t.TeamUsers)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));

            var isUserInTeam = team.ManagerId == token.UserId ||
                              team.TeamUsers.Any(x => x.UserId == token.UserId);

            if (!isUserInTeam)
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(403, ErrorMessages.NotTeamMember));

            return null;
        }
    }
}
