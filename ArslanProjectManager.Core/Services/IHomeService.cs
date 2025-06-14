using ArslanProjectManager.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    public interface IHomeService
    {
        Task<HomeDto> GetHomeSummaryAsync(int userId);
    }
}
