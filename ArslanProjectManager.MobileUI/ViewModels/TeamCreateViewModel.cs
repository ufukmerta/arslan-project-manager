using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class TeamCreateViewModel(TeamService teamService) : ObservableValidator
    {
        [ObservableProperty]
        [Required(ErrorMessage = "Team name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Team name must be between 1 and 50 characters.")]
        public partial string Name { get; set; }

        [ObservableProperty]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public partial string Description { get; set; }

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; }

        [ObservableProperty]
        public partial string ValidationMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool HasValidationErrors { get; set; }

        [ObservableProperty]
        public partial string NameError { get; set; }

        partial void OnNameChanged(string value)
        {
            ValidateProperty(value, nameof(Name));
            NameError = GetErrors(nameof(Name)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            UpdateValidationState();
        }

        partial void OnDescriptionChanged(string value)
        {
            ValidateProperty(value, nameof(Description));
            UpdateValidationState();
        }

        private void UpdateValidationState()
        {
            var errors = GetErrors().ToList();
            HasValidationErrors = errors.Any();
            ValidationMessage = errors.FirstOrDefault()?.ErrorMessage ?? string.Empty;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task CreateAsync()
        {
            if (IsLoading) return;

            ErrorMessage = string.Empty;
            ValidationMessage = string.Empty;
            HasValidationErrors = false;

            ValidateAllProperties();
            UpdateValidationState();

            if (HasValidationErrors)
            {
                return;
            }

            IsLoading = true;

            try
            {
                var teamCreateDto = new TeamCreateDto
                {
                    TeamName = Name,
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                    ManagerId = 0 // Will be set by the API from the token
                };

                var response = await teamService.CreateTeamAsync(teamCreateDto);
                if (response != null && response.IsSuccess)
                {
                    await Shell.Current.GoToAsync($"..?refresh=true");
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to create team. Please try again.";
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
        private static async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..", true);
        }
    }
}

