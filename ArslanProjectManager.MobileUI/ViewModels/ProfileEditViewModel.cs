using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.MobileUI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class ProfileEditViewModel(UserService userService) : ObservableValidator
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        [Required()]
        [StringLength(50)]
        private string name = string.Empty;

        [ObservableProperty]
        [Required()]
        [EmailAddress(ErrorMessage = ErrorMessages.InvalidEmailFormat)]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = ErrorMessages.InvalidEmailFormat)]
        private string email = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string nameError = string.Empty;

        [ObservableProperty]
        private string emailError = string.Empty;

        partial void OnNameChanged(string value)
        {
            ValidateProperty(value, nameof(Name));
            NameError = GetErrors(nameof(Name)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
        }

        partial void OnEmailChanged(string value)
        {
            ValidateProperty(value, nameof(Email));
            EmailError = GetErrors(nameof(Email)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
        }

        private void ClearAllErrors()
        {
            NameError = string.Empty;
            EmailError = string.Empty;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        public async Task LoadProfileAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var response = await userService.GetUpdateProfileAsync();
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    Id = response.Data.Id;
                    Name = response.Data.Name ?? string.Empty;
                    Email = response.Data.Email ?? string.Empty;
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load profile.";
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
        public async Task UpdateProfileAsync()
        {
            if (IsLoading) return;

            ClearAllErrors();
            ValidateAllProperties();

            NameError = GetErrors(nameof(Name)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            EmailError = GetErrors(nameof(Email)).FirstOrDefault()?.ErrorMessage ?? string.Empty;

            if (HasErrors)
            {
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var updateDto = new UserUpdateDto { Id = Id, Name = Name, Email = Email };
                var response = await userService.UpdateProfileAsync(updateDto);
                if (response != null && response.IsSuccess)
                {
                    //if there is no error, we need to show a success message.
                    await Shell.Current.DisplayAlert("Success", "Profile updated successfully", "OK");
                    await Shell.Current.GoToAsync("..", true);
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to update profile.";
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
        public static async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..", true);
        }

        [RelayCommand]
        public static async Task NavigateToChangePasswordAsync()
        {
            await Shell.Current.GoToAsync(nameof(ChangePasswordPage));
        }

        public IRelayCommand SaveCommand => UpdateProfileCommand;
    }
} 