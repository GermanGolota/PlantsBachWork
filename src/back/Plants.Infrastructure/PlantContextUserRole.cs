using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Plants.Core.Entities;
using Plants.Shared;
using System;

namespace Plants.Infrastructure
{
    public partial class PlantsContext : DbContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            var converter = new UserRoleConverter();
            modelBuilder.Entity<CurrentUserRole>()
                .Property(x => x.RoleName)
                .HasColumnName("rolename")
                .HasConversion(converter);
            modelBuilder.Entity<PersonToLogin>()
               .Property(x => x.Login)
               .HasColumnName("login")
               .HasConversion(new LoginConverter());
        }
    }

    public class LoginConverter : ValueConverter<string, string>
    {
        public LoginConverter() : base(v => v, v => v)
        {

        }
    }

    public class UserRoleConverter : ValueConverter<UserRole, string>
    {
        public UserRoleConverter() : base(v => From(v), v => To(v))
        {
        }

        private static string From(UserRole role)
        {
            return role switch
            {
                UserRole.Consumer => "consumer",
                UserRole.Producer => "producer",
                UserRole.Manager => "manager",
                _ => throw new NotImplementedException(),
            };
        }

        private static UserRole To(string role)
        {
            return role switch
            {
                "consumer" => UserRole.Consumer,
                "producer" => UserRole.Producer,
                "manager" => UserRole.Manager,
                _ => throw new ArgumentException("Bad role name", role)
            };
        }
    }
}
