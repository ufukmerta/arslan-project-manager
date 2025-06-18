using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Service.Exceptions
{
    public class ClientSideException: Exception
    {
        public ClientSideException(string message) : base(message)
        {
        }
        public ClientSideException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public ClientSideException() : base("A client-side error occurred.")
        {
        }
    }
}
