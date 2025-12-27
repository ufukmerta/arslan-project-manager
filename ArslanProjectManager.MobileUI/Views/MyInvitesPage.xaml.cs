using ArslanProjectManager.MobileUI.ViewModels;
using ArslanProjectManager.Core.DTOs;
using CommunityToolkit.Mvvm.Input;

namespace ArslanProjectManager.MobileUI.Views
{
    public partial class MyInvitesPage : ContentPage
    {
        private readonly MyInvitesViewModel _viewModel;

        public MyInvitesPage(MyInvitesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }

        private void OnAcceptInviteClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is PendingInviteDto invite)
            {
                _viewModel.AcceptInviteCommand.Execute(invite);
            }
        }

        private void OnRejectInviteClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is PendingInviteDto invite)
            {
                _viewModel.RejectInviteCommand.Execute(invite);
            }
        }
    }
}

