using Microsoft.AspNet.Identity;
using MVC_BugTraker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_BugTraker.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var UserId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(p => p.Id == UserId);
           var DashBaord = new DashBaord();
            DashBaord.TotallUsers = db.Users.Count();
            DashBaord.TotallProjects = db.Projects.Count();
            DashBaord.TotallTickets = db.Tickets.Count();
            DashBaord.TotallComments = db.TicketsComments.Count();
            DashBaord.UserTickets = user.TicketsCreated.Count();

            return View(DashBaord);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}