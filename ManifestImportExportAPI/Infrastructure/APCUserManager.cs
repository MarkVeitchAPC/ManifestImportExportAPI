using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Repositories;
using ManifestImportExportAPI.Repositories.RepositoryInterfaces;
using Microsoft.AspNet.Identity;

namespace ManifestImportExportAPI.Infrastructure
{
    public class APCUserManager : UserManager<APCUser, string>
    {
        public APCUserManager(IUserStore<APCUser, string> store) : base(store)
        {
        }

        public static APCUserManager Create()
        {
            var repo = (IAPCAuthRepository)UnityConfig.GetConfiguredContainer().Resolve(typeof(IAPCAuthRepository), "UserRepo");
            var userManager = new APCUserManager(repo);
            return userManager;
        }
    }
}