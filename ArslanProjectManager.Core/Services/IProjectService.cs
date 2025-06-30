using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    public interface IProjectService: IGenericService<Project>
    {
        Task<ProjectDetailsDto?> GetProjectDetailsAsync(int id);
    }
}
