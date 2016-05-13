using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Infrastructure;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace ManifestImportExportAPI.Provider
{
    public class APCOAuthProvider : OAuthAuthorizationServerProvider
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            logger.Log(LogLevel.Debug, "GrantResourceOwnerCredentials");
            var allowedOrigin = "*";

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            var userManager = context.OwinContext.GetUserManager<APCUserManager>();

            logger.Log(LogLevel.Debug, "Authorising user {0}.", context.UserName);
            APCUser user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null)
            {
                logger.Log(LogLevel.Debug, "User: {0} username or password is incorrect.", context.UserName);
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            ClaimsIdentity oAuthIdentity = new ClaimsIdentity("JWT");
            var roleStrings = await userManager.GetRolesAsync(user.Id);
            var temp = new List<string>(roleStrings);
            var roles = temp.Select(x => new Claim(ClaimTypes.Role, x));
            oAuthIdentity.AddClaims(roles);
            var claims = await userManager.GetClaimsAsync(user.Id);
            oAuthIdentity.AddClaims(claims);

            var ticket = new AuthenticationTicket(oAuthIdentity, null);

            context.Validated(ticket);
            logger.Log(LogLevel.Debug, "User: {0} is authorised", context.UserName);

        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

    }
}