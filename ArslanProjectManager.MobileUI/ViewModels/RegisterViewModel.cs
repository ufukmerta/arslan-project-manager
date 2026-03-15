using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.Core.Constants;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class RegisterViewModel(AuthService authService) : ObservableValidator
    {
        [ObservableProperty]
        [Required]
        [StringLength(50)]
        public partial string Name { get; set; }

        [ObservableProperty]
        [Required]
        [EmailAddress]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = ErrorMessages.InvalidEmailFormat)]
        public partial string Email { get; set; }

        [ObservableProperty]
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*/?&+\-_.])[A-Za-z\d@$!%*/?&+\-_.]{8,}$", ErrorMessage = ErrorMessages.InvalidPasswordFormat)]
        [DataType(DataType.Password)]
        public partial string Password { get; set; }

        [ObservableProperty]
        [DataType(DataType.Password)]
        public partial string ConfirmPassword { get; set; }


        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; }

        [ObservableProperty]
        public partial string NameError { get; set; }

        [ObservableProperty]
        public partial string EmailError { get; set; }

        [ObservableProperty]
        public partial string PasswordError { get; set; }

        [ObservableProperty]
        public partial string ConfirmPasswordError { get; set; }

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

        partial void OnPasswordChanged(string value)
        {
            ValidateProperty(value, nameof(Password));
            PasswordError = GetErrors(nameof(Password)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
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
            else if (!Password.Equals(ConfirmPassword))
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
            NameError = string.Empty;
            EmailError = string.Empty;
            PasswordError = string.Empty;
            ConfirmPasswordError = string.Empty;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsLoading) return;

            ClearAllErrors();
            ValidateAllProperties();

            NameError = GetErrors(nameof(Name)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            EmailError = GetErrors(nameof(Email)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            PasswordError = GetErrors(nameof(Password)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            ValidateConfirmPassword();

            if (HasErrors || !string.IsNullOrEmpty(ConfirmPasswordError))
            {
                return;
            }

            IsLoading = true;

            try
            {
                var registerDto = new UserCreateDto { Name = Name, Email = Email, Password = Password };
                var response = await authService.RegisterAsync(registerDto);
                if (response != null && response.IsSuccess)
                {
                    await Shell.Current.DisplayAlertAsync("Success", $"Account Created!", "OK");
                    await Shell.Current.GoToAsync($"..?email={Email}");
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Registration failed. Please check your inputs.";
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