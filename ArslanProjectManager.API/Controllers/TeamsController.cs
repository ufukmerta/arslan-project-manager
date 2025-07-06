using ArslanProjectManager.API.Filters;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.ViewModels;
using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Repository;
using ArslanProjectManager.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ArslanProjectManager.Core.Models.TeamInvite;

namespace ArslanProjectManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController(ProjectManagerDbContext context, ITokenService tokenService, ITeamService teamService, ITeamInviteService teamInviteService, IUserService userService) : CustomBaseController(tokenService)
    {
        private readonly ProjectManagerDbContext _context = context;
        private readonly ITeamService _teamService = teamService;
        private readonly ITeamInviteService _teamInviteService = teamInviteService;
        private readonly IUserService _userService = userService;

        [HttpGet()]
        [Authorize]
        public async Task<IActionResult> GetByToken()
        {
            var token = await GetToken();
            if (token == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

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

        [HttpGet("[action]/{id}")]
        [Authorize]
        [ServiceFilter(typeof(NotFoundFilter<Team>))]
        public async Task<IActionResult> Details(int id)
        {
            Token? token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

            var validationResult = ValidateModel(id, x => x > 0, ErrorMessages.InvalidTeamId);
            if (validationResult != null) return validationResult;

            var doesTeamExist = await _teamService.AnyAsync(x => x.Id == id);
            if (!doesTeamExist)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

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

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Create(TeamCreateDto model)
        {
            Token? token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

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

        [HttpGet("[action]/{id}")]
        [Authorize]
        public async Task<IActionResult> Invite(int id)
        {
            Token? token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }
            var user = await _userService.GetByIdAsync(token.UserId);

            var team = await _context.Teams
               .Include(x => x.Manager)
               .Include(x => x.TeamUsers)
               .ThenInclude(x => x.User)
               .FirstOrDefaultAsync(x => x.Id == id);

            if (team is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var teamInviteCreateViewDto = new TeamInviteCreateViewDto
            {
                TeamId = team.Id,
                TeamName = team.TeamName,
                InviterName = user.Name,
            };

            return CreateActionResult(CustomResponseDto<TeamInviteCreateViewDto>.Success(teamInviteCreateViewDto, 200));
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Invite(TeamInviteCreateDto model)
        {
            Token? token = await GetToken();
            if (token is null)
            {
                 return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

            var team = await _context.Teams
                .Include(x => x.TeamUsers)
                .ThenInclude(x => x.User)
                .Include(x => x.TeamInvites)
                .FirstOrDefaultAsync(x => x.Id == model.TeamId);

            if (team is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            var teamAccessResult = await ValidateTeamAccess(model.TeamId, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var invitedUser = await _userService.GetByEmail(model.InvitedEmail);

            if (invitedUser != null && team.TeamUsers.Any(x => x.UserId == invitedUser.Id))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.UserAlreadyTeamMember));
            }

            if (team.TeamInvites.Any(x => x.InvitedEmail == model.InvitedEmail && x.Status == InviteStatus.Pending))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvitationAlreadySent));
            }

            var teamInvite = new TeamInvite
            {
                TeamId = model.TeamId,
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

        [HttpGet("[action]/{id}")]
        [Authorize]
        public async Task<IActionResult> Invites(int id)
        {
            Token? token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

            var validationResult = ValidateModel(id, x => x > 0, ErrorMessages.InvalidTeamId);
            if (validationResult != null) return validationResult;

            var team = await _context.Teams
                .Include(x => x.TeamUsers)
                .Include(x => x.TeamInvites)
                .ThenInclude(x => x.InvitedBy)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (team is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.TeamNotFound));
            }

            var teamAccessResult = await ValidateTeamAccess(id, token.UserId);
            if (teamAccessResult != null) return teamAccessResult;

            var teamInvites = team.TeamInvites
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

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> CancelInvite(CancelInviteDto model)
        {
            Token? token = await GetToken();
            if (token is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(401, ErrorMessages.Unauthorized));
            }

            if (model.InviteId <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidInviteId));
            }

            var teamInvite = await _context.TeamInvites
                .Include(x => x.Team)
                .FirstOrDefaultAsync(x => x.Id == model.InviteId);

            if (teamInvite is null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.InvitationNotFound));
            }

            if (teamInvite.Status != InviteStatus.Pending)
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
            var token = await GetToken();
            if (token?.UserId != userId)
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
