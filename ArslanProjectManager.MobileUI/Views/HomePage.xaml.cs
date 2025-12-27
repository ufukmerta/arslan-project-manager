using ArslanProjectManager.MobileUI.ViewModels;
using ArslanProjectManager.Core.DTOs;

namespace ArslanProjectManager.MobileUI.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        Loaded += async (s, e) => await _viewModel.LoadHomeAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadHomeAsync();
    }

    private void OnTaskTapped(object? sender, EventArgs e)
    {
        if (sender is Border border && border.BindingContext is RecentTaskDto task)
        {
            _viewModel.OpenTaskDetailCommand.ExecuteAsync(task);
        }
    }

    private void OnProjectTapped(object? sender, EventArgs e)
    {
        if (sender is Border border && border.BindingContext is RecentProjectDto project)
        {
            _viewModel.OpenProjectDetailCommand.ExecuteAsync(project);
        }
    }
}