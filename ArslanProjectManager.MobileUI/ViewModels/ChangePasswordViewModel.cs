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
        public partial string CurrentPassword { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessages.PasswordsRequired)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = ErrorMessages.InvalidPasswordFormat)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*/?&+\-_.])[A-Za-z\d@$!%*/?&+\-_.]{8,}$", ErrorMessage = ErrorMessages.InvalidPasswordFormat)]
        [DataType(DataType.Password)]
        public partial string NewPassword { get; set; }

        [ObservableProperty]
        [DataType(DataType.Password)]
        public partial string ConfirmPassword { get; set; }

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; }

        [ObservableProperty]
        public partial string CurrentPasswordError { get; set; }

        [ObservableProperty]
        public partial string NewPasswordError { get; set; }

        [ObservableProperty]
        public partial string ConfirmPasswordError { get; set; }

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
                    await Shell.Current.DisplayAlertAsync("Success", "Password changed successfully.", "OK");
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
