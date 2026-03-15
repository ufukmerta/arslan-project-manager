using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.MobileUI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class ProjectDetailViewModel(ProjectService projectService) : ObservableObject
    {
        [ObservableProperty]
        public partial ProjectDetailsDto? ProjectDetails { get; set; }

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<MiniProjectTaskDto> ToDoTasks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<MiniProjectTaskDto> InProgressTasks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<MiniProjectTaskDto> DoneTasks { get; set; }

        partial void OnProjectDetailsChanged(ProjectDetailsDto? value)
        {
            if (value?.Tasks != null)
            {
                ToDoTasks = [.. value.Tasks.Where(t => t.BoardId == 1 || t.BoardId == 0)];
                InProgressTasks = [.. value.Tasks.Where(t => t.BoardId == 2)];
                DoneTasks = [.. value.Tasks.Where(t => t.BoardId == 3)];
            }
            else
            {
                ToDoTasks = [];
                InProgressTasks = [];
                DoneTasks = [];
            }
        }

        [RelayCommand]
        public async Task LoadProjectDetailsAsync(int id)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var response = await projectService.GetProjectDetailsAsync(id);
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    ProjectDetails = response.Data;
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load project details.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        
        [RelayCommand]
        public async Task OpenTaskDetailAsync(MiniProjectTaskDto task)
        {
            if (task != null)
            {
                await Shell.Current.GoToAsync($"{nameof(TaskDetailPage)}?id={task.Id}");
            }
        }

        /*
        [RelayCommand]
        public async Task AddTaskAsync(object parameter)
        {
            int boardId = Convert.ToInt32(parameter);
            if (ProjectDetails != null)
            {
                await Shell.Current.GoToAsync($"{nameof(TaskCreatePage)}?projectId={ProjectDetails.Id}&boardId={boardId}");
            }
        }*/
    }
}