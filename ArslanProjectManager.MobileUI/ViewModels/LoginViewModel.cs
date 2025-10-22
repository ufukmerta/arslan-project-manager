using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.MobileUI.Views;
using ArslanProjectManager.Core.Constants;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class LoginViewModel(AuthService authService, IAuthStorage authStorage) : ObservableValidator, IQueryAttributable
    {
        private readonly AuthService _authService = authService;
        private readonly IAuthStorage _authStorage = authStorage;

        [ObservableProperty]
        [Required]
        [EmailAddress]
        private string email = string.Empty;

        [ObservableProperty]
        [Required]
        [DataType(DataType.Password)]
        private string password = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;


        [ObservableProperty]
        private string errorMessage = string.Empty;

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsLoading) return;
            ErrorMessage = string.Empty;
            ValidateAllProperties();
            if (HasErrors)
            {
                var firstError = GetErrors().FirstOrDefault();
                ErrorMessage = firstError?.ErrorMessage ?? "Please check your input.";
                return;
            }
            IsLoading = true;
            try
            {
                var loginDto = new UserLoginDto { Email = Email, Password = Password };
                var response = await _authService.LoginAsync(loginDto);
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    var token = response.Data;
                    await _authStorage.SaveTokensAsync(token.AccessToken, token.RefreshToken, token.Expiration, token.RefreshTokenExpiration);
                    await Shell.Current.GoToAsync("//home");
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? ErrorMessages.InvalidCredentials;
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
        private async Task GoToRegisterAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(RegisterPage));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("email", out var value))
            {
                Email = value?.ToString() ?? string.Empty;
            }
        }
    }
}