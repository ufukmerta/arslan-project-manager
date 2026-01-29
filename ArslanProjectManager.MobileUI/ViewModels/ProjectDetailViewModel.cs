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
        private ProjectDetailsDto? projectDetails;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<MiniProjectTaskDto> toDoTasks = [];
        [ObservableProperty]
        private ObservableCollection<MiniProjectTaskDto> inProgressTasks = [];
        [ObservableProperty]
        private ObservableCollection<MiniProjectTaskDto> doneTasks = [];

        partial void OnProjectDetailsChanged(ProjectDetailsDto? value)
        {
            if (value?.Tasks != null)
            {
                ToDoTasks = new(value.Tasks.Where(t => t.BoardId == 1 || t.BoardId == 0));
                InProgressTasks = new(value.Tasks.Where(t => t.BoardId == 2));
                DoneTasks = new(value.Tasks.Where(t => t.BoardId == 3));
            }
            else
            {
                ToDoTasks = new();
                InProgressTasks = new();
                DoneTasks = new();
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
            catch (System.Exception ex)
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