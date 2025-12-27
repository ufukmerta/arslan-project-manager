using ArslanProjectManager.Core.Services;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.MobileUI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;
        private readonly IAuthStorage _authStorage;

        [ObservableProperty] private string? fullName;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? role;
        [ObservableProperty] private string? profileImage;
        [ObservableProperty] private int projectCount;
        [ObservableProperty] private int completedProjects;
        [ObservableProperty] private int taskCount;
        [ObservableProperty] private int completedTasks;
        [ObservableProperty] private string? registerDate;
        [ObservableProperty] private string? errorMessage;
        [ObservableProperty] private string? currentTeam;

        public IRelayCommand LogoutCommand { get; }
        public IRelayCommand EditProfileCommand { get; }
        public IRelayCommand OpenProjectsCommand { get; }
        public IRelayCommand OpenTasksCommand { get; }
        public IRelayCommand OpenInvitationsCommand { get; }

        public ProfileViewModel(UserService userService, AuthService authService, IAuthStorage authStorage)
        {
            _userService = userService;
            _authService = authService;
            _authStorage = authStorage;
            LogoutCommand = new RelayCommand(OnLogout);
            EditProfileCommand = new RelayCommand(OnEditProfile);
            OpenProjectsCommand = new RelayCommand(OnOpenProjects);
            OpenTasksCommand = new RelayCommand(OnOpenTasks);
            OpenInvitationsCommand = new RelayCommand(OnOpenInvitations);
        }

        public async Task LoadProfileAsync()
        {
            try
            {
                var response = await _userService.GetProfileAsync();
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
        }

        private async void OnLogout()
        {
            await _authStorage.ClearTokensAsync();
            await _authService.LogoutAsync();
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