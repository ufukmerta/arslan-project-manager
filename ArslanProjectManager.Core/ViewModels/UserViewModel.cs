using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.ViewModels
{
    public class UserProfileViewModel
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? Picture { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool OwnProfile { get; set; }

        // Project Status Information
        public int TotalProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int ProjectCompletionRate => TotalProjects == 0 ? 0 : (CompletedProjects * 100 / TotalProjects);

        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TaskCompletionRate => TotalTasks == 0 ? 0 : (CompletedTasks * 100 / TotalTasks);

        public string? CurrentTeam { get; set; }
        public string? Role { get; set; } = "Team Member";
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*\/?&+\-_.])[A-Za-z\d@$!%*\/?&+\-_.]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class EditUserViewModel
    {
        public required int Id { get; set; }

        [StringLength(50)]
        public required string Name { get; set; }

        [EmailAddress]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        public string? Picture { get; set; }

        [FileExtensions(Extensions = "jpg, jpeg, png", ErrorMessage = "Please upload a valid image file (jpg, jpeg, png).")]
        [DataType(DataType.Upload)]
        public IFormFile? ProfilePictureFile { get; set; }
    }
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*\/?&+\-_.])[A-Za-z\d@$!%*\/?&+\-_.]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
