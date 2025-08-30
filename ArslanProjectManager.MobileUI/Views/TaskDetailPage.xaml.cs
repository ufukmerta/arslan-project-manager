using ArslanProjectManager.MobileUI.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace ArslanProjectManager.MobileUI.Views;

public partial class TaskDetailPage : ContentPage, IQueryAttributable
{
    private readonly TaskDetailViewModel _viewModel;
    
    public TaskDetailPage(TaskDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && int.TryParse(idObj?.ToString(), out var id))
        {
            await _viewModel.SelectTaskAsync(id);
        }
    }
} 