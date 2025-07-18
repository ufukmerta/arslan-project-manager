namespace ArslanProjectManager.MobileUI.Services
{
    public partial class IgnoreSslHandler : HttpClientHandler
    {
        public IgnoreSslHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        }
    }
} 