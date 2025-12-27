using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.MobileUI.Services.UIServices
{
    public class TeamService(IHttpClientFactory httpClientFactory) : GenericService(httpClientFactory)
    {
        public async Task<CustomResponseDto<IEnumerable<TeamDto>>?> GetMyTeamsAsync()
        {
            var client = base.GetClient();
            var response = await client.GetAsync($"teams");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<IEnumerable<TeamDto>>>();
                return errorWrapper ?? new CustomResponseDto<IEnumerable<TeamDto>> { IsSuccess = false, Errors = ["Failed to load teams"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<IEnumerable<TeamDto>>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<TeamDetailsDto>?> GetTeamDetailsAsync(int id)
        {
            var client = GetClient();
            var response = await client.GetAsync($"teams/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TeamDetailsDto>>();
                return errorWrapper ?? new CustomResponseDto<TeamDetailsDto> { IsSuccess = false, Errors = ["Failed to load team details"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TeamDetailsDto>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<MiniTeamDto>?> CreateTeamAsync(TeamCreateDto dto)
        {
            var client = GetClient();
            var response = await client.PostAsJsonAsync("teams", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<MiniTeamDto>>();
                return errorWrapper ?? new CustomResponseDto<MiniTeamDto> { IsSuccess = false, Errors = ["Failed to create team"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<MiniTeamDto>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<TeamInviteCreateViewDto>?> GetTeamInviteMetaAsync(int teamId)
        {
            var client = GetClient();
            var response = await client.GetAsync($"teams/{teamId}/invite-meta");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TeamInviteCreateViewDto>>();
                return errorWrapper ?? new CustomResponseDto<TeamInviteCreateViewDto> { IsSuccess = false, Errors = ["Failed to load invite information"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<TeamInviteCreateViewDto>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<NoContentDto>?> InviteUserAsync(int teamId, TeamInviteCreateDto dto)
        {
            var client = GetClient();
            var response = await client.PostAsJsonAsync($"teams/{teamId}/invites", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
                return errorWrapper ?? new CustomResponseDto<NoContentDto> { IsSuccess = false, Errors = ["Failed to invite user"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<IEnumerable<TeamInviteListDto>>?> GetTeamInvitesAsync(int teamId)
        {
            var client = GetClient();
            var response = await client.GetAsync($"teams/{teamId}/invites");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<IEnumerable<TeamInviteListDto>>>();
                return errorWrapper ?? new CustomResponseDto<IEnumerable<TeamInviteListDto>> { IsSuccess = false, Errors = ["Failed to load team invites"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<IEnumerable<TeamInviteListDto>>>();
            return wrapper;
        }

        public async Task<CustomResponseDto<NoContentDto>?> CancelInviteAsync(int inviteId)
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"invites/{inviteId}");
            if (!response.IsSuccessStatusCode)
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
                return errorWrapper ?? new CustomResponseDto<NoContentDto> { IsSuccess = false, Errors = ["Failed to cancel invite"] };
            }
            var wrapper = await response.Content.ReadFromJsonAsync<CustomResponseDto<NoContentDto>>();
            return wrapper;
        }
    }
}
