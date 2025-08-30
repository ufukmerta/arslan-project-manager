using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.MobileUI.ViewModels;

namespace ArslanProjectManager.MobileUI.Views;

public partial class TasksPage : ContentPage
{
    private readonly ProjectTaskViewModel _viewModel;
    public TasksPage(ProjectTaskViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadTasksAsync();
        Appearing += async (s, e) => await viewModel.LoadTasksAsync();
    }

    private async void OnTaskSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is ProjectTaskDto selectedTask)
        {
            await _viewModel.OpenTaskDetailAsync(selectedTask);
            ((CollectionView)sender).SelectedItem = null;
        }
    }
} 