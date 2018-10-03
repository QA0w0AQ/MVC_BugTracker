namespace MVC_BugTraker.Migrations
{
    using MVC_BugTraker.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Models.ApplicationDbContext context)
        {

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            if (!context.Roles.Any(r => r.Name == "ProjectManager"))
            {
                roleManager.Create(new IdentityRole { Name = "ProjectManager" });
            }

            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }

            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }


            ApplicationUser admin = null;
            if (!context.Users.Any(p => p.UserName == "admin@gmail.com"))
            {
                admin = new ApplicationUser();
                admin.UserName = "admin@gmail.com";
                admin.Email = "admin@gmail.com";
                admin.FirstName = "Admin";
                admin.LastName = "User";
                admin.DisplayName = "Admin User";

                userManager.Create(admin, "Password-1");
            }
            else
            {
                admin = context.Users.Where(p => p.UserName == "admin@gmail.com")
                    .FirstOrDefault();
            }
            if (!userManager.IsInRole(admin.Id, "Admin"))
            {
                userManager.AddToRole(admin.Id, "Admin");
            }

            ApplicationUser PM = null;
            if (!context.Users.Any(p => p.UserName == "ProjectManager@gmail.com"))
            {
                PM = new ApplicationUser();
                PM.UserName = "ProjectManager@gmail.com";
                PM.Email = "ProjectManager@gmail.com";
                PM.FirstName = "Project";
                PM.LastName = "Manager";
                PM.DisplayName = "PM";

                userManager.Create(PM, "Password-1");
            }
            else
            {
                PM = context.Users.Where(p => p.UserName == "ProjectManager@gmail.com")
                    .FirstOrDefault();
            }
            if (!userManager.IsInRole(PM.Id, "ProjectManager"))
            {
                userManager.AddToRole(PM.Id, "ProjectManager");
            }

            ApplicationUser Del = null;
            if (!context.Users.Any(p => p.UserName == "Developer@gmail.com"))
            {
                Del = new ApplicationUser();
                Del.UserName = "Developer@gmail.com";
                Del.Email = "Developer@gmail.com";
                Del.FirstName = "Developer";
                Del.LastName = "Developer";
                Del.DisplayName = "Developer";

                userManager.Create(Del, "Password-1");
            }
            else
            {
                Del = context.Users.Where(p => p.UserName == "Developer@gmail.com")
                    .FirstOrDefault();
            }
            if (!userManager.IsInRole(Del.Id, "Developer"))
            {
                userManager.AddToRole(Del.Id, "Developer");
            }

            ApplicationUser SubM = null;
            if (!context.Users.Any(p => p.UserName == "Submitter@gmail.com"))
            {
                SubM = new ApplicationUser();
                SubM.UserName = "Submitter@gmail.com";
                SubM.Email = "Submitter@gmail.com";
                SubM.FirstName = "Submitter";
                SubM.LastName = "Submitter";
                SubM.DisplayName = "SB";

                userManager.Create(SubM, "Password-1");
            }
            else
            {
                SubM = context.Users.Where(p => p.UserName == "Submitter@gmail.com")
                    .FirstOrDefault();
            }
            if (!userManager.IsInRole(SubM.Id, "Submitter"))
            {
                userManager.AddToRole(SubM.Id, "Submitter");
            }
        }
    }
}
