using ArslanProjectManager.MobileUI.Views;

namespace ArslanProjectManager.MobileUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ProfileEditPage), typeof(ProfileEditPage));
            Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage));
            Routing.RegisterRoute(nameof(ProjectDetailPage), typeof(ProjectDetailPage));
            Routing.RegisterRoute(nameof(TaskDetailPage), typeof(TaskDetailPage));
            Routing.RegisterRoute(nameof(TeamCreatePage), typeof(TeamCreatePage));
            Routing.RegisterRoute(nameof(TeamDetailPage), typeof(TeamDetailPage));
            Routing.RegisterRoute(nameof(TeamInvitesPage), typeof(TeamInvitesPage));
            Routing.RegisterRoute(nameof(TeamInvitePage), typeof(TeamInvitePage));
            Routing.RegisterRoute(nameof(MyInvitesPage), typeof(MyInvitesPage));
        }
    }
}
