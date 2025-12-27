using ArslanProjectManager.MobileUI.ViewModels;

namespace ArslanProjectManager.MobileUI.Views
{
    public partial class TeamInvitePage : ContentPage
    {
        public TeamInvitePage(TeamInviteViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
} 