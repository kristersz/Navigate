namespace Navigate.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Security;
    using WebMatrix.WebData;

    internal sealed class Configuration : DbMigrationsConfiguration<Navigate.Models.NavigateDb>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Navigate.Models.NavigateDb context)
        {
            SeedMembership();
        }

        private void SeedMembership()
        {
            if (!WebSecurity.Initialized)
            {
                WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfiles", "UserId", "UserName", autoCreateTables: true);
            }

            var roles = (SimpleRoleProvider)Roles.Provider;
            var membership = (SimpleMembershipProvider)Membership.Provider;

            if (!roles.RoleExists("Administrator"))
            {
                roles.CreateRole("Administrator");
            }

            if (!roles.RoleExists("User"))
            {
                roles.CreateRole("User");
            }

            if (!WebSecurity.UserExists("m@ster"))
            {
                WebSecurity.CreateUserAndAccount("m@ster", "M@ster12", new { Email = "kristers.zimecs@gmail.com", BaseLocation = "Elizabetes 75, Riga" }, false);
            }

            if (!roles.GetRolesForUser("m@ster").Contains("Administrator"))
            {
                roles.AddUsersToRoles(new[] { "m@ster" }, new[] { "Administrator" });

            }
        }
    }
}
