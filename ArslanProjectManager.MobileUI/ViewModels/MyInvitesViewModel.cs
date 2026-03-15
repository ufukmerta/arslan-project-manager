using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class MyInvitesViewModel(UserService userService) : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<PendingInviteDto> Invites { get; set; }

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await userService.GetMyInvitesAsync();
                if (response?.IsSuccess == true && response.Data != null)
                {
                    Invites = new ObservableCollection<PendingInviteDto>(response.Data);
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load invitations.";
                    Invites = new ObservableCollection<PendingInviteDto>();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Invites = new ObservableCollection<PendingInviteDto>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task AcceptInviteAsync(PendingInviteDto invite)
        {
            if (invite == null) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await userService.AcceptInviteAsync(invite.TeamInviteId);
                if (response?.IsSuccess == true)
                {
                    Invites.Remove(invite);
                    await Shell.Current.DisplayAlertAsync("Success", "You have successfully joined the team!", "OK");
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to accept invitation.";
                    await Shell.Current.DisplayAlertAsync("Error", ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                await Shell.Current.DisplayAlertAsync("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task RejectInviteAsync(PendingInviteDto invite)
        {
            if (invite == null) return;

            var confirmed = await Shell.Current.DisplayAlertAsync(
                "Reject Invitation",
                $"Are you sure you want to reject the invitation from {invite.TeamName}?",
                "Reject",
                "Cancel");

            if (!confirmed) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await userService.RejectInviteAsync(invite.TeamInviteId);
                if (response?.IsSuccess == true)
                {
                    Invites.Remove(invite);
                    await Shell.Current.DisplayAlertAsync("Success", "Invitation rejected successfully.", "OK");
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to reject invitation.";
                    await Shell.Current.DisplayAlertAsync("Error", ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                await Shell.Current.DisplayAlertAsync("Error", ErrorMessage, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

