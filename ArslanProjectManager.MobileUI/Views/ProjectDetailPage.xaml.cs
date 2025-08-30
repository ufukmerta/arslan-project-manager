using ArslanProjectManager.MobileUI.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using ArslanProjectManager.Core.DTOs;

namespace ArslanProjectManager.MobileUI.Views;

public partial class ProjectDetailPage : ContentPage, IQueryAttributable
{
    private readonly ProjectDetailViewModel _viewModel;
    public ProjectDetailPage(ProjectDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && int.TryParse(idObj?.ToString(), out var id))
        {
            await _viewModel.LoadProjectDetailsAsync(id);
        }
    }
    /*
    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (_viewModel.ProjectDetails != null)
            await Shell.Current.GoToAsync($"{nameof(ProjectEditPage)}?id={_viewModel.ProjectDetails.Id}");
    }
    
	private async void OnDeleteClicked(object sender, EventArgs e)
	{
		if (_viewModel.ProjectDetails != null)
			await Shell.Current.GoToAsync($"{nameof(ProjectDeleteConfirmPage)}?id={_viewModel.ProjectDetails.Id}");
	}

	private async void OnBackClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}
    */

	private async void OnTaskSelected(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection?.FirstOrDefault() is MiniProjectTaskDto selectedTask)
		{
			await _viewModel.OpenTaskDetailAsync(selectedTask);
			((CollectionView)sender).SelectedItem = null;
		}
    }
}