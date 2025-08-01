using ArslanProjectManager.MobileUI.ViewModels;

namespace ArslanProjectManager.MobileUI.Views;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage(ChangePasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 