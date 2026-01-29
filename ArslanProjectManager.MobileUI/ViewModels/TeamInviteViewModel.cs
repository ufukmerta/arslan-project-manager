using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class TeamInviteViewModel(TeamService teamService) : ObservableObject, IQueryAttributable
    {   
        [ObservableProperty]
        private int teamId;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string teamName = string.Empty;

        [ObservableProperty]
        private string inviterName = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

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

            if (TeamId > 0)
            {
                MainThread.BeginInvokeOnMainThread(async () => await LoadInviteMetaAsync());
            }
            else
            {
                var page = Shell.Current?.CurrentPage;
                page?.DisplayAlert(
                    "Success",
                    "Invitation sent successfully!",
                    "OK");
                Shell.Current?.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task SendInviteAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Please enter an email address.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var inviteDto = new TeamInviteCreateDto
                {
                    InvitedEmail = Email
                };

                var response = await teamService.InviteUserAsync(TeamId, inviteDto);

                if (response?.IsSuccess == true)
                {
                    var page = Shell.Current?.CurrentPage;
                    if (page != null)
                    {
                        await page.DisplayAlert(
                        "Success",
                        "Invitation sent successfully!",
                        "OK");
                    }
                    await Shell.Current!.GoToAsync("..?refresh=true");
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to send invitation";
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

        [RelayCommand]
        private async Task CancelAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception)
            {
                // Fallback navigation
                await Shell.Current.GoToAsync("//teams");
            }
        }

        private async Task LoadInviteMetaAsync()
        {
            if (TeamId <= 0) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var response = await teamService.GetTeamInviteMetaAsync(TeamId);
                if (response?.IsSuccess == true && response.Data != null)
                {
                    TeamName = response.Data.TeamName;
                    InviterName = response.Data.InviterName;
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load invite information.";
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
    }
} 