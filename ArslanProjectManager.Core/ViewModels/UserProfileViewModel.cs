namespace ArslanProjectManager.Core.ViewModels;

public class UserProfileViewModel
{
    public required string Name { get; set; }   
    public required string Email { get; set; }
    public string? Picture { get; set; } = "profile.png";
    public DateTime RegisterDate { get; set; }
    public bool OwnProfile { get; set; }

    // Project Status Information
    public int TotalProjects { get; set; }
    public int CompletedProjects { get; set; }
    public double ProjectCompletionRate => TotalProjects == 0 ? 0 : (CompletedProjects * 100.0 / TotalProjects);
    
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double TaskCompletionRate => TotalTasks == 0 ? 0 : (CompletedTasks * 100.0 / TotalTasks);
    
    public string? CurrentTeam { get; set; }
    public string? Role { get; set; } = "Team Member";    
}
