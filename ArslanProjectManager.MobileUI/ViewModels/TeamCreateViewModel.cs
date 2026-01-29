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
        private string name = string.Empty;

        [ObservableProperty]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        private string description = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string validationMessage = string.Empty;

        [ObservableProperty]
        private bool hasValidationErrors = false;

        [ObservableProperty]
        private string nameError = string.Empty;

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

