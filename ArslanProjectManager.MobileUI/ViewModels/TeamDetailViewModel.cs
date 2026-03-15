using ArslanProjectManager.Core.DTOs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ArslanProjectManager.MobileUI.Services.UIServices;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ArslanProjectManager.MobileUI.Views;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class TeamDetailViewModel(TeamService teamService) : ObservableObject
    {
        [ObservableProperty]
        public partial int TeamId { get; set; }

        [ObservableProperty]
        public partial string TeamName { get; set; }

        [ObservableProperty]
        public partial string Description { get; set; }

        [ObservableProperty]
        public partial string ManagerName { get; set; }

        [ObservableProperty]
        public partial int ManagerId { get; set; }

        [ObservableProperty]
        public partial int CurrentUserId { get; set; }

        [ObservableProperty]
        public partial bool IsCurrentUserManager { get; set; }

        [ObservableProperty]
        public partial bool CanEditProjects { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<TeamUserDto> Members { get; set; } = [];

        [ObservableProperty]
        public partial ObservableCollection<TeamProjectDto> Projects { get; set; } = [];

        [ObservableProperty]
        public partial ObservableCollection<TeamInviteListDto> PendingInvites { get; set; } = [];

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; }
        
        private CancellationTokenSource _cts = new();

        public void CancelLoading()
        {
            _cts.Cancel();
        }

        public async Task LoadTeamDetails(int id)
        {
            TeamId = id;
            if (TeamId <= 0)
            {
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await teamService.GetTeamDetailsAsync(TeamId);

                if (response?.IsSuccess == true && response.Data != null)
                {
                    var details = response.Data;
                    TeamName = details.TeamName ?? string.Empty;
                    Description = details.Description ?? string.Empty;
                    ManagerName = details.ManagerName ?? string.Empty;
                    ManagerId = details.ManagerId;
                    CanEditProjects = details.CanEditProjects;

                    Members.Clear();
                    if (details.Members != null && details.Members.Count > 0)
                    {
                        foreach (var member in details.Members)
                        {
                            if (member != null)
                            {
                                Members.Add(member);
                            }
                        }
                        OnPropertyChanged(nameof(Members));
                    }
                    else
                    {
                        OnPropertyChanged(nameof(Members));
                    }

                    Projects.Clear();
                    if (details.Projects != null && details.Projects.Count > 0)
                    {
                        foreach (var project in details.Projects)
                        {
                            if (project != null)
                            {
                                Projects.Add(project);
                            }
                        }

                        OnPropertyChanged(nameof(Projects));
                    }
                    else
                    {
                        OnPropertyChanged(nameof(Projects));
                    }
                }
                else
                {
                    TeamId = 0;
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load team details";
                }
            }
            catch (Exception ex)
            {
                TeamId = 0;
                ErrorMessage = $"Error loading team details: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ManageInvitesAsync()
        {
            if (Shell.Current == null) return;
            await Shell.Current.GoToAsync($"{nameof(TeamInvitesPage)}?id={TeamId}");
        }

        [RelayCommand]
        private async Task InviteAsync()
        {
            try
            {
                if (Shell.Current == null) return;
                await Shell.Current.GoToAsync($"{nameof(TeamInvitePage)}?id={TeamId}");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task ProjectTappedAsync(TeamProjectDto project)
        {
            if (project == null || Shell.Current == null) return;

            try
            {
                await Shell.Current.GoToAsync($"{nameof(ProjectDetailPage)}?id={project.Id}");
            }
            catch (Exception ex)
            {
                var page = Shell.Current?.CurrentPage;
                if (page != null)
                {
                    await page.DisplayAlertAsync(
                        "Navigation Error",
                        $"Could not open project: {ex.Message}",
                        "OK");
                }
            }
        }
    }
}