using Dapper;
using Microsoft.EntityFrameworkCore;
using Plants.Application.Commands;
using Plants.Application.Contracts;
using Plants.Application.Requests;
using Plants.Core;
using Plants.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plants.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly PlantsContextFactory _ctx;

        public UserService(PlantsContextFactory contextFactory)
        {
            _ctx = contextFactory;
        }

        public async Task AddRole(string login, UserRole role)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                var converter = new UserRoleConverter();
                var roleStr = converter.ConvertToProvider(role) as string;

                using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "CALL add_user_to_group(@login, @role::UserRoles);";
                    var p = new
                    {
                        login,
                        role = roleStr
                    };
                    await connection.ExecuteAsync(sql, p);
                }
            }
        }

        public async Task<CreateUserResult> CreateUser(string Login, List<UserRole> roles, string FirstName,
            string LastName, string PhoneNumber, string Password)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                using (var connection = ctx.Database.GetDbConnection())
                {
                    var Roles = ConvertRoles(roles.ToArray());
                    string sql = "CALL create_user(@Login, @Password, @Roles::UserRoles[], @FirstName, @LastName, @PhoneNumber);";
                    var p = new
                    {
                        Login,
                        Password,
                        Roles,
                        FirstName,
                        LastName,
                        PhoneNumber
                    };
                    CreateUserResult res;
                    try
                    {
                        await connection.ExecuteAsync(sql, p);
                        res = new CreateUserResult(true, "Successfully created user!");
                    }
                    catch (Exception e)
                    {
                        res = new CreateUserResult(false, e.Message);
                    }
                    return res;
                }
            }
        }

        public async Task RemoveRole(string login, UserRole role)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                var converter = new UserRoleConverter();
                var roleStr = converter.ConvertToProvider(role) as string;

                using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "CALL remove_user_from_group(@login, @role::UserRoles);";
                    var p = new
                    {
                        login,
                        role = roleStr
                    };
                    await connection.ExecuteAsync(sql, p);
                }
            }
        }

        public async Task<IEnumerable<FindUsersResultItem>> SearchFor(string FullName, string Contact, UserRole[] roles)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                if (roles?.Any() == false)
                {
                    roles = null;
                }

                if (String.IsNullOrEmpty(FullName))
                {
                    FullName = null;
                }

                if (String.IsNullOrEmpty(Contact))
                {
                    Contact = null;
                }
                string[] Roles = ConvertRoles(roles);

                using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT * FROM search_users(@FullName, @Contact, @Roles::UserRoles[])";
                    var p = new
                    {
                        FullName,
                        Contact,
                        Roles
                    };
                    return await connection.QueryAsync<FindUsersResultItem>(sql, p);
                }
            }
        }

        private static string[] ConvertRoles(UserRole[] roles)
        {
            var converter = new UserRoleConverter();
            var Roles = roles?.Select(x => converter.ConvertToProvider(x) as string)?.ToArray();
            return Roles;
        }

        public async Task ChangeMyPassword(string newPassword)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = $"ALTER ROLE session_user WITH ENCRYPTED password '{newPassword.Replace("'", "''")}';";
                    await connection.ExecuteAsync(sql);
                }
            }
        }
    }
}
