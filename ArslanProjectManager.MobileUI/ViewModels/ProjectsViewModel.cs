using ArslanProjectManager.Core.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ArslanProjectManager.MobileUI.Services.UIServices;
using ArslanProjectManager.MobileUI.Views;

namespace ArslanProjectManager.MobileUI.ViewModels
{
    public partial class ProjectsViewModel(ProjectService projectService) : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<UserProjectDto> projects = [];

        private IEnumerable<UserProjectDto> allProjects = [];

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Projects = new ObservableCollection<UserProjectDto>(allProjects);
            }
            else
            {
                var filtered = allProjects.Where(p =>
                    (!string.IsNullOrEmpty(p.ProjectName) && p.ProjectName.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.TeamName) && p.TeamName.Contains(value, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                Projects = new ObservableCollection<UserProjectDto>(filtered);
            }
        }



        [RelayCommand]
        public async Task LoadProjectsAsync()
        {
            if (IsLoading)
            {
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var response = await projectService.GetAllProjectsAsync();
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    allProjects = [.. response.Data];
                    Projects = new ObservableCollection<UserProjectDto>(allProjects);
                }
                else
                {
                    ErrorMessage = response?.Errors?.FirstOrDefault() ?? "Failed to load projects.";
                    allProjects = [];
                    Projects = [];
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
                allProjects = [];
                Projects = [];
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task CreateProjectAsync()
        {
            // To be implemented => await Shell.Current.GoToAsync($"{nameof(ProjectCreatePage)}");
        }
    }
} 