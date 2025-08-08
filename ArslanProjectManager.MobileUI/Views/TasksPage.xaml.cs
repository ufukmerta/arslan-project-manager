using ArslanProjectManager.MobileUI.ViewModels;

namespace ArslanProjectManager.MobileUI.Views;

public partial class TasksPage : ContentPage
{
    public TasksPage(ProjectTaskViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadTasksAsync();
        Appearing += async (s, e) => await viewModel.LoadTasksAsync();
    }
} 