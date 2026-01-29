using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class TeamInvitesViewModel(TeamService teamService) : ObservableObject, IQueryAttributable
    {
        private bool _isInitialized;

        [ObservableProperty]
        private int teamId;

        [ObservableProperty]
        private string teamName = string.Empty;

        [ObservableProperty]
        private string inviterName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<TeamInviteListDto> invites = [];

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query == null || query.Count == 0) return;

            if (query.TryGetValue("id", out var value))
            {
                if (value is int intId)
                {
                    TeamId = intId;
                }
                else if (value is string stringId && int.TryParse(stringId, out var parsed))
                {
                    TeamId = parsed;
                }
            }

            if (TeamId > 0 && !_isInitialized)
            {
                MainThread.BeginInvokeOnMainThread(async () => await InitializeAsync());
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (IsLoading) return;
            IsRefreshing = true;
            await InitializeAsync(forceReload: true);
            IsRefreshing = false;
        }

        [RelayCommand]
        private async Task SendInviteAsync()
        {
            if (TeamId <= 0) return;
            await Shell.Current.GoToAsync($"{nameof(Views.TeamInvitePage)}?id={TeamId}");
        }

        [RelayCommand]
        private async Task CancelInviteAsync(TeamInviteListDto invite)
        {
            if (invite == null || invite.Status.ToString() != "Pending") return;
            var page = Shell.Current?.CurrentPage;
            if (page == null) return;

            var confirm = await page.DisplayAlert(
                "Cancel Invite",
                $"Cancel invite for {invite.InvitedEmail}?",
                "Yes",
                "No");

            if (!confirm) return;

            IsLoading = true;
            try
            {
                ErrorMessage = string.Empty;
                var response = await teamService.CancelInviteAsync(invite.Id);
                if (response?.IsSuccess == true)
                {
                    Invites.Remove(invite);
                    await page.DisplayAlert("Success", $"Invite for {invite.InvitedEmail} canceled.", "OK");
                    // Refresh the invites list to get updated data
                    await LoadInvitesAsync();
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to cancel invite";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task InitializeAsync(bool forceReload = false)
        {
            if (TeamId <= 0) return;
            if (_isInitialized && !forceReload) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await LoadInviteMetaAsync();
                await LoadInvitesAsync();

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadInviteMetaAsync()
        {
            var response = await teamService.GetTeamInviteMetaAsync(TeamId);
            if (response?.IsSuccess == true && response.Data != null)
            {
                TeamName = response.Data.TeamName;
                InviterName = response.Data.InviterName;
            }
            else
            {
                ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load invite information";
            }
        }

        private async Task LoadInvitesAsync()
        {
            var response = await teamService.GetTeamInvitesAsync(TeamId);
            if (response?.IsSuccess == true && response.Data != null)
            {
                Invites.Clear();
                foreach (var invite in response.Data)
                {
                    Invites.Add(invite);
                }
            }
            else
            {
                ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load invites";
            }
        }
    }
}

