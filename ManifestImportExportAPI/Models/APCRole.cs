using ManifestImportExportAPI.Domain;
using Microsoft.AspNet.Identity;

namespace ManifestImportExportAPI.Models
{
    public enum APCRoleType
    {
        Depot, Consignor, ThirdParty, InPost
    }
    public class APCRole : IRole<string>
    {
        APCRoleType _type;
        public APCRole() { }
        public APCRole(string roleName)
        {
            _type = Parse.ParseRoleType(roleName);
        }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}