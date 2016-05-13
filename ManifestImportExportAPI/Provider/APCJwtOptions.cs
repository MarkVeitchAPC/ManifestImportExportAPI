using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using System.Configuration;

namespace ManifestImportExportAPI.Provider
{
    public class APCJwtOptions : JwtBearerAuthenticationOptions
    {
        public APCJwtOptions()
        {
            var issuer = ConfigurationManager.AppSettings["host"];
            var audience = ConfigurationManager.AppSettings["as:AudienceId"];
            byte[] audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

            AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active;
            AllowedAudiences = new[] { audience };
            IssuerSecurityTokenProviders = new[] { new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret) };
        }
    }
}