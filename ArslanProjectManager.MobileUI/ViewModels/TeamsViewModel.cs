using ArslanProjectManager.Core.ViewModels;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.MobileUI.Views;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class TeamsViewModel(TeamService teamService, IMapper mapper) : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<TeamViewModel> teams = [];

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        private readonly TeamService _teamService = teamService;
        private readonly IMapper _mapper = mapper;

        private IEnumerable<TeamViewModel> allTeams = [];

        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Teams = new ObservableCollection<TeamViewModel>(allTeams);
            }
            else
            {
                var filtered = allTeams.Where(t =>
                    (!string.IsNullOrEmpty(t.TeamName) && t.TeamName.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(value, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                Teams = new ObservableCollection<TeamViewModel>(filtered);
            }
        }

        public async Task LoadTeamsAsync()
        {
            try
            {
                var response = await _teamService.GetMyTeamsAsync();
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    allTeams = _mapper.Map<IEnumerable<TeamViewModel>>(response.Data);
                    Teams = new ObservableCollection<TeamViewModel>(allTeams);
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load teams.";
                    allTeams = [];
                    Teams = [];
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                allTeams = [];
                Teams = [];
            }
        }

        [RelayCommand]
        public async Task CreateTeamAsync()
        {
            await Shell.Current.GoToAsync(nameof(TeamCreatePage));
        }

        public async Task OpenTeamDetailAsync(TeamViewModel selectedTeam)
        {
            await Shell.Current.GoToAsync($"{nameof(TeamDetailPage)}?id={selectedTeam.TeamId}");
        }
    }
}