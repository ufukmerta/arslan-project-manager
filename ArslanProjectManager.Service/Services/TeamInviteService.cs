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
    public class TeamInviteService : GenericService<TeamInvite>, ITeamInviteService
    {
        public TeamInviteService(IGenericRepository<TeamInvite> repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
        }
    }
}
