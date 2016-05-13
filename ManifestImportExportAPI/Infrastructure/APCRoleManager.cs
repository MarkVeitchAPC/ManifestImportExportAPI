using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Repositories;
using Microsoft.AspNet.Identity;

namespace ManifestImportExportAPI.Infrastructure
{
    public class APCRoleManager : RoleManager<APCRole>
    {
        public APCRoleManager(IRoleStore<APCRole, string> roleStore) : base(roleStore)
        {

        }

        public static APCRoleManager Create()
        {
            //var appRoleManager = new APCRoleManager(new MockAPCAuthRepository());
            var appRoleManager = new APCRoleManager(new APCAuthRepository());

            return appRoleManager;
        }
    }
}