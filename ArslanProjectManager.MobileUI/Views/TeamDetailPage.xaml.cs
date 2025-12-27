using ArslanProjectManager.MobileUI.ViewModels;
using ArslanProjectManager.Core.DTOs;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace ArslanProjectManager.MobileUI.Views
{
    public partial class TeamDetailPage : ContentPage, IQueryAttributable
    {
        private readonly TeamDetailViewModel _viewModel;

        public TeamDetailPage(TeamDetailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var idObj) && int.TryParse(idObj?.ToString(), out var id))
            {
                await _viewModel.LoadTeamDetails(id);
            }
        }

        private async void OnProjectTapped(object? sender, EventArgs e)
        {
            if (sender is Border border && border.BindingContext is TeamProjectDto project)
            {
                await _viewModel.ProjectTappedCommand.ExecuteAsync(project);
            }
        }
    }
} 