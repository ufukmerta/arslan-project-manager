using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.MobileUI.Services.UIServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class TaskDetailViewModel(ProjectTaskService projectTaskService) : ObservableObject
    {
        [ObservableProperty]
        private ProjectTaskDto? selectedTask;

        [ObservableProperty]
        private ObservableCollection<TaskCommentDto> comments = [];

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string newComment = string.Empty;

        [ObservableProperty]
        private bool isCommentError = false;

        [RelayCommand]
        public async Task SelectTaskAsync(int id)
        {
            IsLoading = true;
            IsCommentError = false;
            ErrorMessage = string.Empty;
            try
            {
                var response = await projectTaskService.GetTaskDetailsAsync(id);
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    SelectedTask = response.Data;
                    Comments = new ObservableCollection<TaskCommentDto>(SelectedTask.Comments.OrderByDescending(x => x.CreatedDate));
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load task details.";
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
        public async Task AddCommentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewComment) || SelectedTask == null)
            {
                IsCommentError = true;
                ErrorMessage = "Please enter a comment.";
                return;
            }

            try
            {
                var response = await projectTaskService.AddCommentAsync(SelectedTask.Id, NewComment);
                if (response != null && response.IsSuccess)
                {
                    NewComment = string.Empty;
                    // Refresh the task to get the comments after adding a new comment
                    await SelectTaskAsync(SelectedTask.Id);
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to add comment.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
} 