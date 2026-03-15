using ArslanProjectManager.Core.Services;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.MobileUI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class ProfileViewModel(UserService userService, AuthService authService, IAuthStorage authStorage) : ObservableObject
    {
        [ObservableProperty]
        public partial string? FullName { get; set; }

        [ObservableProperty]
        public partial string? Email { get; set; }

        [ObservableProperty]
        public partial string? Role { get; set; }

        [ObservableProperty]
        public partial string? ProfileImage { get; set; }

        [ObservableProperty]
        public partial int ProjectCount { get; set; }

        [ObservableProperty]
        public partial int CompletedProjects { get; set; }

        [ObservableProperty]
        public partial int TaskCount { get; set; }

        [ObservableProperty]
        public partial int CompletedTasks { get; set; }

        [ObservableProperty]
        public partial string? RegisterDate { get; set; }

        [ObservableProperty]
        public partial string? ErrorMessage { get; set; }

        [ObservableProperty]
        public partial string? CurrentTeam { get; set; }

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        private IRelayCommand? _logoutCommand;
        private IRelayCommand? _editProfileCommand;
        private IRelayCommand? _openProjectsCommand;
        private IRelayCommand? _openTasksCommand;
        private IRelayCommand? _openInvitationsCommand;

        public IRelayCommand LogoutCommand => _logoutCommand ??= new RelayCommand(OnLogout);
        public IRelayCommand EditProfileCommand => _editProfileCommand ??= new RelayCommand(OnEditProfile);
        public IRelayCommand OpenProjectsCommand => _openProjectsCommand ??= new RelayCommand(OnOpenProjects);
        public IRelayCommand OpenTasksCommand => _openTasksCommand ??= new RelayCommand(OnOpenTasks);
        public IRelayCommand OpenInvitationsCommand => _openInvitationsCommand ??= new RelayCommand(OnOpenInvitations);

        public async Task LoadProfileAsync()
        {
            if (IsLoading)
            {
                return;
            }

            IsLoading = true;
            try
            {
                var response = await userService.GetProfileAsync();
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    var profile = response.Data;
                    FullName = profile.Name;
                    Email = profile.Email;
                    ProjectCount = profile.TotalProjects;
                    CompletedProjects = profile.CompletedProjects;
                    TaskCount = profile.TotalTasks;
                    CompletedTasks = profile.CompletedTasks;
                    CurrentTeam = profile.CurrentTeam ?? string.Empty;
                    RegisterDate = profile.RegisterDate.ToString("dd.MM.yyyy");

                    if (profile.Role == "Team Manager")
                    {
                        Role = $"Team Manager of {CurrentTeam}";
                    }
                    else
                    {
                        Role = "Team Member";
                    }
                    if (profile.ProfilePicture == @"/img/profile.png" || string.IsNullOrEmpty(profile.ProfilePicture))
                    {
                        ProfileImage = "profile.png";
                    }
                    else
                    {
                        ProfileImage = profile.ProfilePicture;
                    }

                    
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load profile.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnLogout()
        {
            await authStorage.ClearTokensAsync();
            await authService.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }

        private async void OnEditProfile()
        {
            await Shell.Current.GoToAsync(nameof(ProfileEditPage));
        }

        private async void OnOpenProjects()
        {
            await Shell.Current.GoToAsync("//projects");
        }
        private async void OnOpenTasks()
        {
            await Shell.Current.GoToAsync("//tasks");
        }
        private async void OnOpenInvitations()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(MyInvitesPage));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load invites: {ex.Message}");
            }
        }
    }
}