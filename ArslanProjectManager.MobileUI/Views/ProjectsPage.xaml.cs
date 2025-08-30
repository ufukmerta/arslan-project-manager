using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.MobileUI.ViewModels;

namespace ArslanProjectManager.MobileUI.Views;

public partial class ProjectsPage : ContentPage
{
    public ProjectsPage(ProjectsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadProjectsAsync();
        Appearing += async (s, e) => await viewModel.LoadProjectsAsync();
    }

    private async void OnProjectSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is UserProjectDto project)
        {
            await Shell.Current.GoToAsync($"{nameof(ProjectDetailPage)}?id={project.ProjectId}");
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}