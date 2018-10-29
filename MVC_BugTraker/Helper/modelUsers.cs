using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MVC_BugTraker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_BugTraker.Helper
{
    public class modelUsers
    {

        private ApplicationDbContext Db;

        private UserManager<ApplicationUser> UserManager;

        private RoleManager<IdentityRole> RoleManager;

        public modelUsers()
        {
            Db = new ApplicationDbContext();
            UserManager = new
            UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Db));
            RoleManager = new
            RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Db));
        }

        public List<IdentityRole> GetAllRoles()
        {
            return RoleManager.Roles.ToList();
        }

        public List<string> GetUserRoles(string id)
        {
            return UserManager.GetRoles(id).ToList();
        }

    }
}