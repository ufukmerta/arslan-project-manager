using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.MobileUI.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace ArslanProjectManager.MobileUI.Views
{
    public partial class TeamInvitesPage : ContentPage
    {
        private readonly TeamInvitesViewModel _viewModel;

        public TeamInvitesPage(TeamInvitesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync(forceReload: true);
        }

        private async void OnCancelInviteClicked(object sender, EventArgs e)
        {
            if (sender is not Button button) return;
            if (button.BindingContext is not TeamInviteListDto invite) return;

            if (_viewModel.CancelInviteCommand is IAsyncRelayCommand<TeamInviteListDto> command &&
                command.CanExecute(invite))
            {
                await command.ExecuteAsync(invite);
            }
        }
    }
}

