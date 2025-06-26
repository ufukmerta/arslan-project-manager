using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Service.Services
{
    public class TaskLogService: GenericService<TaskLog>, ITaskLogService
    {
        public TaskLogService(IGenericRepository<TaskLog> repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
        }
    }    
}
