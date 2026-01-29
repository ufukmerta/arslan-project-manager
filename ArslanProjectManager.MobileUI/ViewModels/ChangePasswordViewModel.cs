using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.Core.Constants;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class ChangePasswordViewModel(UserService userService) : ObservableValidator
    {
        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessages.PasswordsRequired)]
        private string currentPassword = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessages.PasswordsRequired)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = ErrorMessages.InvalidPasswordFormat)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*/?&+\-_.])[A-Za-z\d@$!%*/?&+\-_.]{8,}$", ErrorMessage = ErrorMessages.InvalidPasswordFormat)]
        [DataType(DataType.Password)]
        private string newPassword = string.Empty;

        [ObservableProperty]
        [DataType(DataType.Password)]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string currentPasswordError = string.Empty;

        [ObservableProperty]
        private string newPasswordError = string.Empty;

        [ObservableProperty]
        private string confirmPasswordError = string.Empty;

        partial void OnCurrentPasswordChanged(string value)
        {
            ValidateProperty(value, nameof(CurrentPassword));
            CurrentPasswordError = GetErrors(nameof(CurrentPassword)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
        }

        partial void OnNewPasswordChanged(string value)
        {
            ValidateProperty(value, nameof(NewPassword));
            NewPasswordError = GetErrors(nameof(NewPassword)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            if (!string.IsNullOrEmpty(ConfirmPassword))
            {
                ValidateConfirmPassword();
            }
        }

        partial void OnConfirmPasswordChanged(string value)
        {
            ValidateConfirmPassword();
        }

        private void ValidateConfirmPassword()
        {
            if (string.IsNullOrEmpty(ConfirmPassword))
            {
                ConfirmPasswordError = "Please confirm your password.";
            }
            else if (!NewPassword.Equals(ConfirmPassword))
            {
                ConfirmPasswordError = "Passwords do not match.";
            }
            else
            {
                ConfirmPasswordError = string.Empty;
            }
        }

        private void ClearAllErrors()
        {
            CurrentPasswordError = string.Empty;
            NewPasswordError = string.Empty;
            ConfirmPasswordError = string.Empty;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        public async Task ChangePasswordAsync()
        {
            if (IsLoading) return;

            ClearAllErrors();
            ValidateAllProperties();

            CurrentPasswordError = GetErrors(nameof(CurrentPassword)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            NewPasswordError = GetErrors(nameof(NewPassword)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            ConfirmPasswordError = GetErrors(nameof(ConfirmPassword)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            ValidateConfirmPassword();

            if (HasErrors || !string.IsNullOrEmpty(ConfirmPasswordError))
            {
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await userService.ChangePasswordAsync(CurrentPassword, NewPassword);
                if (response != null && response.IsSuccess)
                {
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;
                    await Shell.Current.DisplayAlert("Success", "Password changed successfully.", "OK");
                    await Shell.Current.GoToAsync("..", true);
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to change password.";
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
