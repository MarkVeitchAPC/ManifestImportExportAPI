using Microsoft.AspNet.Identity;
using System.Collections.Generic;

namespace ManifestImportExportAPI.Models
{
    public class APCUser : IUser<string>
    {
        public APCUser() { }
        public APCUser(string userName) { }
        public string Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string DepotNumber { get; set; }
        public IList<string> Roles { get; set; }
        public IList<string> Accounts { get; set; }
    }
}