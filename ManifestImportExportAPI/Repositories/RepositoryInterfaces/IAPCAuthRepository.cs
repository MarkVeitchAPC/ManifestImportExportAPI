using ManifestImportExportAPI.Models;
using Microsoft.AspNet.Identity;

namespace ManifestImportExportAPI.Repositories.RepositoryInterfaces
{
    public interface IAPCAuthRepository :
        IUserStore<APCUser, string>,
        IUserPasswordStore<APCUser, string>,
        IUserRoleStore<APCUser, string>,
        IUserClaimStore<APCUser, string>,
        IRoleStore<APCRole, string>
    {
    }
}