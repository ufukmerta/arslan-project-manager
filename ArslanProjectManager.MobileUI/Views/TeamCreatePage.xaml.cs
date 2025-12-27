using ArslanProjectManager.MobileUI.ViewModels;

namespace ArslanProjectManager.MobileUI.Views;

public partial class TeamCreatePage : ContentPage
{
    public TeamCreatePage(TeamCreateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
