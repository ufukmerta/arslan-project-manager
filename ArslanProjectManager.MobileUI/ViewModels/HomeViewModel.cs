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
        private bool _isLoading;

        [ObservableProperty]
        public partial int TotalProjects { get; set; }

        [ObservableProperty]
        public partial int CompletedProjects { get; set; }

        [ObservableProperty]
        public partial decimal ProjectCompletionRate { get; set; }

        [ObservableProperty]
        public partial int TotalTasks { get; set; }

        [ObservableProperty]
        public partial int CompletedTasks { get; set; }

        [ObservableProperty]
        public partial decimal TaskCompletionRate { get; set; }

        [ObservableProperty]
        public partial int TotalTeams { get; set; }

        [ObservableProperty]
        public partial int TotalMembers { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<RecentTaskDto> RecentTasks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<RecentProjectDto> RecentProjects { get; set; }

        [ObservableProperty]
        public partial string? ErrorMessage { get; set; }

        [RelayCommand]
        public async Task LoadHomeAsync()
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;
            try
            {
                var response = await homeService.GetHomeSummaryAsync();
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    var homeDto = response.Data;
                    TotalProjects = homeDto.TotalProjects;
                    CompletedProjects = homeDto.CompletedProjects;
                    ProjectCompletionRate = TotalProjects == 0 ? 0 : decimal.Divide(CompletedProjects, TotalProjects);
                    TotalTasks = homeDto.TotalTasks;
                    CompletedTasks = homeDto.CompletedTasks;
                    TaskCompletionRate = TotalTasks == 0 ? 0 : decimal.Divide(CompletedTasks, TotalTasks);
                    TotalTeams = homeDto.TotalTeams;
                    TotalMembers = homeDto.TotalMembers;
                    RecentTasks = [.. homeDto.RecentTasks];
                    RecentProjects = [.. homeDto.RecentProjects];
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
            finally
            {
                _isLoading = false;
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