using ManifestImportExportAPI.Infrastructure;
using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Repositories;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace ManifestImportExportAPI.Controllers
{
    public class BaseApiController : ApiController
    {
        private APCUserManager _userManager = null;
        private APCRoleManager _roleManager = null;

        protected APCUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<APCUserManager>();
            }
        }

        protected APCRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? Request.GetOwinContext().GetUserManager<APCRoleManager>();
            }
        }

        public BaseApiController()
        {
        }

        protected IEnumerable<System.Security.Claims.Claim> UserClaims()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var claims = identity.Claims;
            return claims;
        }
        protected IList<string> UserAccounts()
        {
            var claims = UserClaims();
            var accounts = claims.Where(x => x.Type.Equals("accountIDs")).Select(x => x.Value).ToList();
            return accounts;
        }

        protected string UserDepotNumber()
        {
            var claims = UserClaims();
            var depot = claims.Where(x => x.Type.Equals("depot")).Select(x => x.Value).First();
            return depot;
        }

        protected bool IsUserDepot()
        {
            return User.IsInRole(APCRoleType.Depot.ToString());
        }

        protected bool IsUserConsignor()
        {
            return User.IsInRole(APCRoleType.Consignor.ToString());
        }

        protected bool IsUserThirdParty()
        {
            return User.IsInRole(APCRoleType.ThirdParty.ToString());
        }

        protected bool NoDataForAllResults(params IQueryStatus[] statuses)
        {
            return statuses.All(x => x.Status == QueryStatus.NO_DATA);
        }

        protected bool IsInPost()
        {
            return User.IsInRole(APCRoleType.InPost.ToString());
        }

    }
}