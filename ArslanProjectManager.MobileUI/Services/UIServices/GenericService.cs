using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.MobileUI.Services.UIServices
{
    public class GenericService(IHttpClientFactory httpClientFactory)
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        protected HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient("ArslanProjectManagerAPI");
            return client;
        }
    }
}
