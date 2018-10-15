using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MVC_BugTraker.Models;

namespace MVC_BugTraker.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            var tickets = db.Tickets.Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(tickets.ToList());
        }

        public ActionResult AdminIndex()
        {
            var tickets = db.Tickets.Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View("AdminIndex",tickets.ToList());
        }
        [Authorize(Roles ="Submitter")]
        public ActionResult SubmitterIndex()
        {
            var userId = User.Identity.GetUserId();
            var tickets = db.Tickets.
                Where(p=>p.OwnerUserId==userId).
                Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View("SubmitterIndex",tickets.ToList());
        }

        [Authorize(Roles = "ProjectManager")]
        public ActionResult PMIndex()
        {
            var userId = User.Identity.GetUserId();

            var tickets = db.Users.Where(p => p.Id == userId).FirstOrDefault().
                 Projects.SelectMany(p => p.Tickets);

            return View("SoloIndex", tickets.ToList());
        }

        [Authorize(Roles = "Developer")]
        public ActionResult DelIndex1()
        {
            var userId = User.Identity.GetUserId();

            var tickets = db.Users.Where(p => p.Id == userId).FirstOrDefault().
                 Projects.SelectMany(p => p.Tickets);

            return View("Index", tickets.ToList());
        }

        [Authorize(Roles = "Developer")]
        public ActionResult DelIndex2()
        {
            var userId = User.Identity.GetUserId();
            var tickets = db.Tickets.
                Where(p => p.AssignedToUserId == userId).
                Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View("SubmitterIndex", tickets.ToList());
        }
        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tickets tickets = db.Tickets.Find(id);
            if (tickets == null)
            {
                return HttpNotFound();
            }
            return View(tickets);
        }

        // GET: Tickets/Create
        public ActionResult Create()
        {
            ViewBag.ProjectId = new SelectList(db.Projects,"Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId")] Tickets tickets/*ProjectsAssign model*/)
        {
            if (ModelState.IsValid)
            {
                var user = User.Identity.GetUserId();
                tickets.OwnerUserId = user;

                //var project = db.Projects.FirstOrDefault(p => p.Id == model.Id);
                //tickets.AssignedToUserId = project.Users.GetUserId();

                tickets.Created = DateTimeOffset.Now;
                db.Tickets.Add(tickets);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", tickets.Id);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name", tickets.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", tickets.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name", tickets.TicketTypeId);
            return View(tickets);
        }

        // GET: Tickets/Edit/5

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tickets tickets = db.Tickets.Find(id);
            if (tickets == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", tickets.Id);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name", tickets.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", tickets.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name", tickets.TicketTypeId);
            return View(tickets);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId")] Tickets tickets)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tickets).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", tickets.Id);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "Name", tickets.OwnerUserId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name", tickets.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", tickets.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name", tickets.TicketTypeId);
            return View(tickets);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tickets tickets = db.Tickets.Find(id);
            if (tickets == null)
            {
                return HttpNotFound();
            }
            return View(tickets);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tickets tickets = db.Tickets.Find(id);
            db.Tickets.Remove(tickets);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,ProjectManager")]
        public ActionResult Assign(int id)
        {
            var model = new ProjectsAssign();

            model.Id = id;
            var ticket = db.Tickets.FirstOrDefault(p => p.Id == id);
            var users = db.Users.ToList();
            var userIdsAssignedToProject = ticket.Users.Select(p => p.Id).ToList();

            model.UserList = new MultiSelectList(users, "Id", "DisplayName", userIdsAssignedToProject);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProjectManager")]
        public ActionResult Assign(ProjectsAssign model)
        {
            var ticket = db.Tickets.FirstOrDefault(p => p.Id == model.Id);
            var assignedUsers = ticket.Users.ToList();

            foreach (var user in assignedUsers)
            {
                ticket.Users.Remove(user);
            }

            if (model.SelectedUsers != null)
            {
                foreach (var userId in model.SelectedUsers)
                {
                    var user = db.Users.FirstOrDefault(p => p.Id == userId);

                    ticket.Users.Add(user);
                    ticket.AssignedToUserId = userId;
                }
            }
            
            db.SaveChanges();

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
