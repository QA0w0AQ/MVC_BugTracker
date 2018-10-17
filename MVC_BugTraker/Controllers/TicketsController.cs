using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MVC_BugTraker.Helper;
using MVC_BugTraker.Models;

namespace MVC_BugTraker.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        #region Different Roles Index
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

            return View("AdminIndex", tickets.ToList());
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
        #endregion
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

        #region PM And Dev Edit
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
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,AssignedToUserId")] Tickets tickets)
        {
            if (ModelState.IsValid)
            {
                var MyTicket = db.Tickets.First(p => p.Id == tickets.Id);
                MyTicket.Id = tickets.Id;
                MyTicket.Title = tickets.Title;
                MyTicket.Description = tickets.Description;
                MyTicket.Updated = DateTimeOffset.Now;
                MyTicket.TicketTypeId = tickets.TicketTypeId;
                MyTicket.TicketPriorityId = tickets.TicketPriorityId;
                MyTicket.TicketStatusId = tickets.TicketStatusId;


                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", tickets.Id);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name", tickets.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", tickets.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name", tickets.TicketTypeId);
            return View(tickets);
        }
        #endregion

        #region Admin Edit
        [Authorize(Roles = "Admin")]
        public ActionResult ADEdit(int? id)
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
            return View("Edit",tickets);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ADEdit([Bind(Include = "Id,Title,Description,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,AssignedToUserId")] Tickets tickets)
        {
            if (ModelState.IsValid)
            {
                var MyTicket = db.Tickets.First(p => p.Id == tickets.Id);
                MyTicket.Id = tickets.Id;
                MyTicket.Title = tickets.Title;
                MyTicket.Description = tickets.Description;
                MyTicket.Updated = DateTimeOffset.Now;
                MyTicket.TicketTypeId = tickets.TicketTypeId;
                MyTicket.TicketPriorityId = tickets.TicketPriorityId;
                MyTicket.TicketStatusId = tickets.TicketStatusId;
                MyTicket.ProjectId = tickets.ProjectId;

                db.SaveChanges();
                return RedirectToAction("AdminIndex");
            }
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", tickets.Id);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name", tickets.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", tickets.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name", tickets.TicketTypeId);
            return View("Edit", tickets);
        }
        #endregion

        #region Submitter Edit
        [Authorize(Roles = "Submitter")]
        public ActionResult SBEdit(int? id)
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Submitter")]
        public ActionResult SBEdit([Bind(Include = "Id,Title,Description,Updated")] Tickets tickets)
        {
            if (ModelState.IsValid)
            {
                var MyTicket = db.Tickets.First(p => p.Id == tickets.Id);
                MyTicket.Id = tickets.Id;
                MyTicket.Title = tickets.Title;
                MyTicket.Description = tickets.Description;
                MyTicket.Updated = DateTimeOffset.Now;

                db.SaveChanges();
                return RedirectToAction("SubmitterIndex");
            }
            return View(tickets);
        }
        #endregion

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

        [HttpPost]
        public ActionResult CreateComment(int id, string comment)
        {

            var comments = db.Tickets.Where(p => p.Id == id).FirstOrDefault();

            if (comments == null)
            {
                return HttpNotFound();
            }
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Comment is empty!";
                return View("Details", new { id });
            }
            var ticketsComment = new TicketsComment();
            ticketsComment.UsersId = User.Identity.GetUserId();
            ticketsComment.Created = DateTime.Now;
            ticketsComment.TicketsId = comments.Id;
            ticketsComment.Comment = comment;

            db.TicketsComments.Add(ticketsComment);
            db.SaveChanges();
            return RedirectToAction("Details", new { id });
        }
        //[HttpPost]
        //public ActionResult CreateComment([Bind(Include ="Id,TicketsId,created,comment,UserId")]TicketsComment comment)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var comments = db.Tickets.Where(p => p.Id == id).FirstOrDefault();


        //        comment.UsersId = User.Identity.GetUserId();
        //        comment.TicketsId = comments.Id;
        //        comment.Created = DateTimeOffset.Now;
        //        db.TicketsComments.Add(comment);
        //        db.SaveChanges();
        //        return RedirectToAction("Details");
        //    }
        //}

        [HttpPost]
        public ActionResult CreateAttachment(int id, string description, HttpPostedFileBase image)
        {

            var attachments = db.Tickets.Where(p => p.Id == id).FirstOrDefault();
            var ticketsAttachment = new TicketAttachment();
            if (attachments == null)
            {
                return HttpNotFound();
            }
            if (ImageUploadValidator.IsWebFriendlyImage(image))
            {
                var fileName = Path.GetFileName(image.FileName);
                image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                ticketsAttachment.MediaURL = "/Uploads/" + fileName;
            }
           
            ticketsAttachment.Created = DateTime.Now;
            ticketsAttachment.FileName = image.FileName;
            ticketsAttachment.Description = description;
            ticketsAttachment.TicketsId = attachments.Id;
            ticketsAttachment.UsersId = User.Identity.GetUserId();
            

            db.TicketAttachments.Add(ticketsAttachment);
            db.SaveChanges();
            return RedirectToAction("Details", new { id });
        }

        public ActionResult DownloadAttachment(int id)
        {
            var attachments = db.TicketAttachments.Where(p => p.Id == id).FirstOrDefault();

            string filepath = Server.MapPath("~/" + attachments.MediaURL);
            byte[] filedata = System.IO.File.ReadAllBytes(filepath);
            string contentType = MimeMapping.GetMimeMapping(filepath);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = attachments.FileName,
                Inline = false,
            };

            Response.AppendHeader("Content-Disposition", cd.ToString());

            return File(filedata, contentType);
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
