using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class ProjectTaskViewModel : ObservableObject
    {
        private readonly ProjectTaskService _projectTaskService;

        [ObservableProperty]
        private ObservableCollection<ProjectTaskDto> tasks = [];

        private List<ProjectTaskDto> allTasks = [];

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string searchText = string.Empty;

        public ProjectTaskViewModel(ProjectTaskService projectTaskService)
        {
            _projectTaskService = projectTaskService;
            Tasks = [];
            allTasks = [];
        }

        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Tasks = new ObservableCollection<ProjectTaskDto>(allTasks);
            }
            else
            {
                var filtered = allTasks.Where(t =>
                    (!string.IsNullOrEmpty(t.ProjectName) && t.ProjectName.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(t.TaskName) && t.TaskName.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(t.AppointeeName) && t.AppointeeName.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(t.AppointerName) && t.AppointerName.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(t.BoardName) && t.BoardName.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(t.TaskCategoryName) && t.TaskCategoryName.Contains(value, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                Tasks = new ObservableCollection<ProjectTaskDto>(filtered);
            }
        }

        [RelayCommand]
        public async Task LoadTasksAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var response = await _projectTaskService.GetAllTasksAsync();
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    allTasks = [.. response.Data];
                    Tasks = new ObservableCollection<ProjectTaskDto>(allTasks);
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load tasks.";
                    allTasks = [];
                    Tasks = [];
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
                allTasks = [];
                Tasks = [];
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task ViewTaskAsync(int id)
        {
            // To be implemented => await Shell.Current.GoToAsync($"{nameof(ProjectTaskDetailPage)}?id={id}");
        }

        [RelayCommand]
        public async Task EditTaskAsync(int id)
        {
            // To be implemented => await Shell.Current.GoToAsync($"{nameof(ProjectTaskEditPage)}?id={id}");
        }

        [RelayCommand]
        public async Task DeleteTaskAsync(int id)
        {
            // To be implemented => await Shell.Current.GoToAsync($"{nameof(ProjectTaskDeletePage)}?id={id}");
        }
    }
}