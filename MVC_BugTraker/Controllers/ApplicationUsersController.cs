using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using MVC_BugTraker.Models;

namespace MVC_BugTraker.Controllers
{
    
    public class ApplicationUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ApplicationUsers
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RolesChange(string id)
        {
            var model = new UserRoles();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            var user = db.Users.Find(id);

            model.Id = id;
            model.Name = User.Identity.Name;

            var roles = roleManager.Roles.ToList();
            var userRole = userManager.GetRoles(id);

            model.Roles = new MultiSelectList(roles, "Name", "Name", userRole);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult RolesChange(UserRoles model)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            var user = userManager.FindById(model.Id);
            var userRoles = userManager.GetRoles(user.Id);
            foreach (var role in userRoles)
            {
                userManager.RemoveFromRole(user.Id, role);
            }


            foreach (var role in model.SelectedRoles)
            {
                userManager.AddToRole(user.Id, role);
            }

            var signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            signInManager.SignIn(user, isPersistent: false, rememberBrowser: false);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
