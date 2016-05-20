using ManifestImportExportAPI.Domain;
using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Repositories.RepositoryInterfaces;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace ManifestImportExportAPI.Repositories
{
    public class APCAuthRepository : IAPCAuthRepository
    {
        private ImmutableDictionary<string, APCUser> _users;
        private string _connectionString;

        public APCAuthRepository()
        {
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["Main.ConnectionString"];
            if (_users == null) { var queryStatus = RetrieveAPCUserToken(); }
        }

        public QueryStatus RetrieveAPCUserToken()
        {
            var queryStatus = QueryStatus.OK;
            var lastClient = string.Empty;
            var hasher = new PasswordHasher();
            var builder = ImmutableDictionary.CreateBuilder<string, APCUser>();
            var newUser = new APCUser();

            APCUser apcUser = new APCUser();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    var cmd = new SqlCommand("dbo.APCTrackerWebAPIGetAuthorisation", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    var count = 0;

                    var dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            var clientName = Parse.ParseString(dr["Client"].ToString());
                            if (clientName != lastClient)
                            {
                                newUser = new APCUser
                                {
                                    Id = Parse.ParseString(dr["Id"].ToString()),
                                    UserName = Parse.ParseString(dr["Client"].ToString()),
                                    PasswordHash = Parse.ParseString(dr["PasswordHash"].ToString()),
                                    DepotNumber = Parse.ParseString(dr["RequestDepotNumber"].ToString()),
                                    Roles = new List<string> { (Convert.ToString(Enum.Parse(typeof(APCRoleType), (dr["UserType"].ToString())))) },
                                    Accounts = new List<string> { dr["APCAccountNumber"].ToString() ?? string.Empty }
                                };
                                builder.Add(count.ToString(), newUser);
                                lastClient = clientName;
                            }
                            else
                            {
                                count--;
                                var lastAdded = (count).ToString();
                                newUser = new APCUser();
                                newUser = builder.GetValueOrDefault(lastAdded);
                                string APCAccountNumber = Parse.ParseString(dr["APCAccountNumber"].ToString()) ?? string.Empty;
                                if (APCAccountNumber != string.Empty) newUser.Accounts.Add(APCAccountNumber);
                                builder.Remove((count).ToString());
                                builder.Add(count.ToString(), newUser);
                            }
                            count++;
                        }
                        _users = builder.ToImmutableDictionary();
                    }
                    else
                    {
                        queryStatus = QueryStatus.NO_DATA;
                    }
                }
                catch (InvalidOperationException ex)
                {

                    queryStatus = QueryStatus.FAILED_CONNECTION;
                    builder.Clear();
                }
                catch (SqlException ex)
                {

                    queryStatus = QueryStatus.FAIL;
                    builder.Clear();
                }
            }

            return queryStatus;
        }

        public Task AddClaimAsync(APCUser user, Claim claim)
        {
            throw new NotImplementedException();
        }

        public Task AddToRoleAsync(APCUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(APCRole role)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(APCUser user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(APCRole role)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(APCUser user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public Task<APCUser> FindByIdAsync(string userId)
        {
            var user = _users[userId];
            return Task.FromResult<APCUser>(user);
        }

        public Task<APCUser> FindByIdAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<APCUser> FindByNameAsync(string userName)
        {
            APCUser user = new APCUser();
            if (_users != null)
            {
                user = _users.Values.Where(x => x.UserName.Equals(userName)).First();
            }
            return Task.FromResult<APCUser>(user);
        }

        public Task<IList<Claim>> GetClaimsAsync(APCUser user)
        {
            var accountIds = user.Accounts.ToList();
            var depotNumber = user.DepotNumber;
            var claims = accountIds.Select(x => new Claim("accountIDs", x)).ToList();
            claims.Add(new Claim("depot", depotNumber));

            return Task.FromResult<IList<Claim>>(claims);
        }

        public Task<string> GetPasswordHashAsync(APCUser user)
        {
            //// add the next two lines into here, debug the wsc service and then bust into it using a Fiddler POST http://localhost:65215/api/oauth/token 
            //var hasher = new PasswordHasher();
            ////var ExpressandGlobal = hasher.HashPassword("dh4CmBZaxQ");
            //var Depot175 = hasher.HashPassword("BrJ8VdFU5a");
            return Task.FromResult(user.PasswordHash);
        }

        public Task<IList<string>> GetRolesAsync(APCUser user)
        {
            return Task<IList<string>>.FromResult(user.Roles);
        }

        public Task<bool> HasPasswordAsync(APCUser user)
        {
            return Task.FromResult(string.IsNullOrEmpty(user.PasswordHash));
        }

        public Task<bool> IsInRoleAsync(APCUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimAsync(APCUser user, Claim claim)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromRoleAsync(APCUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(APCUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task UpdateAsync(APCRole role)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(APCUser user)
        {
            throw new NotImplementedException();
        }

        Task<APCRole> IRoleStore<APCRole, string>.FindByIdAsync(string roleId)
        {
            throw new NotImplementedException();
        }

        Task<APCRole> IRoleStore<APCRole, string>.FindByNameAsync(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}