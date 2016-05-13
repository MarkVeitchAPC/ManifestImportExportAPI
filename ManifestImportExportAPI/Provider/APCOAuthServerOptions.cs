using ManifestImportExportAPI.Domain;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Configuration;

namespace ManifestImportExportAPI.Provider
{
    public class APCOAuthServerOptions : OAuthAuthorizationServerOptions
    {
        public APCOAuthServerOptions()
        {
            #if DEBUG
            AllowInsecureHttp = true;
            #endif
            var oAuthPath = ConfigurationManager.AppSettings["oauthPath"];
            TokenEndpointPath = new PathString(oAuthPath);
            var expiry = ConfigurationManager.AppSettings["tokenExpiryMinutes"];
            AccessTokenExpireTimeSpan = TimeSpan.FromMinutes((double)Parse.ParseInt(expiry));
            var host = ConfigurationManager.AppSettings["host"];
            AccessTokenFormat = new APCJwtFormat(host);
            Provider = new APCOAuthProvider();
        }
    }
}