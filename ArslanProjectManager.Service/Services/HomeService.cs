using ArslanProjectManager.Core;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Service.Services
{
    public class HomeService : IHomeService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public HomeService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public Task<HomeDto> GetHomeSummaryAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
