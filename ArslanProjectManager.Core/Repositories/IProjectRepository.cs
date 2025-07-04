﻿using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Repositories
{
    public interface IProjectRepository: IGenericRepository<Project>
    {
        Task<Project?> GetProjectWithDetailsAsync(int id);
    }
}
