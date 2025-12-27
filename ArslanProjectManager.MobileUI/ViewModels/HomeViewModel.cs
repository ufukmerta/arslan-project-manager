using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.MobileUI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class HomeViewModel(HomeService homeService) : ObservableObject
    {
        private readonly HomeService _homeService = homeService;

        [ObservableProperty] private int totalProjects;
        [ObservableProperty] private int completedProjects;
        [ObservableProperty] private decimal projectCompletionRate;

        [ObservableProperty] private int totalTasks;
        [ObservableProperty] private int completedTasks;
        [ObservableProperty] private decimal taskCompletionRate;

        [ObservableProperty] private int totalTeams;
        [ObservableProperty] private int totalMembers;

        [ObservableProperty] private ObservableCollection<RecentTaskDto> recentTasks = [];
        [ObservableProperty] private ObservableCollection<RecentProjectDto> recentProjects = [];

        [ObservableProperty] private string? errorMessage;

        [RelayCommand]
        public async Task LoadHomeAsync()
        {
            try
            {
                var response = await _homeService.GetHomeSummaryAsync();
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    var homeDto = response.Data;
                    TotalProjects = homeDto.TotalProjects;
                    CompletedProjects = homeDto.CompletedProjects;
                    ProjectCompletionRate = TotalProjects == 0 ? 0 : Decimal.Divide(CompletedProjects, TotalProjects);
                    TotalTasks = homeDto.TotalTasks;
                    CompletedTasks = homeDto.CompletedTasks;
                    TaskCompletionRate = TotalTasks == 0 ? 0 : Decimal.Divide(CompletedTasks, TotalTasks);
                    TotalTeams = homeDto.TotalTeams;
                    TotalMembers = homeDto.TotalMembers;
                    RecentTasks = new ObservableCollection<RecentTaskDto>(homeDto.RecentTasks);
                    RecentProjects = new ObservableCollection<RecentProjectDto>(homeDto.RecentProjects);
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load home data.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        public async Task OpenProjectsAsync()
        {
            await Shell.Current.GoToAsync("//projects");
        }

        [RelayCommand]
        public async Task OpenTasksAsync()
        {
            await Shell.Current.GoToAsync("//tasks");
        }

        [RelayCommand]
        public async Task OpenTeamsAsync()
        {
            await Shell.Current.GoToAsync("//teams");
        }

        [RelayCommand]
        public async Task OpenTaskDetailAsync(RecentTaskDto task)
        {
            if (task is not null)
                await Shell.Current.GoToAsync($"{nameof(TaskDetailPage)}?id={task.Id}");
        }

        [RelayCommand]
        public async Task OpenProjectDetailAsync(RecentProjectDto project)
        {
            if (project is not null)
                await Shell.Current.GoToAsync($"{nameof(ProjectDetailPage)}?id={project.Id}");
        }
    }
}