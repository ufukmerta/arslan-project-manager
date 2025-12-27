using ArslanProjectManager.MobileUI.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.Core.ViewModels;

namespace ArslanProjectManager.MobileUI.Views;

public partial class TeamsPage : ContentPage, IQueryAttributable
{
    private readonly TeamsViewModel _viewModel;

    public TeamsPage(TeamsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTeamsAsync();
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("refresh", out var refreshObj) && bool.TryParse(refreshObj?.ToString(), out var shouldRefresh) && shouldRefresh)
        {
            await _viewModel.LoadTeamsAsync();
        }
    }

    private async void OnTeamSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is TeamViewModel selectedTeam)
        {
            await _viewModel.OpenTeamDetailAsync(selectedTeam);
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}