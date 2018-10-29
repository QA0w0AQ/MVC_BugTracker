using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
            return View("AdminIndex", tickets.ToList());
        }

        [Authorize(Roles = "Submitter")]
        public ActionResult SubmitterIndex()
        {
            var userId = User.Identity.GetUserId();
            var tickets = db.Tickets.
                Where(p => p.OwnerUserId == userId).
                Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View("SubmitterIndex", tickets.ToList());
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
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
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
                var changes = new List<TickectsHistory>();
                var MyTicket = db.Tickets.First(p => p.Id == tickets.Id);
                MyTicket.Id = tickets.Id;
                MyTicket.Title = tickets.Title;
                MyTicket.Description = tickets.Description;
                MyTicket.Updated = DateTimeOffset.Now;
                MyTicket.TicketTypeId = tickets.TicketTypeId;
                MyTicket.TicketPriorityId = tickets.TicketPriorityId;
                MyTicket.TicketStatusId = tickets.TicketStatusId;


                var originalValues = db.Entry(MyTicket).OriginalValues;
                var currentValues = db.Entry(MyTicket).CurrentValues;

                foreach (var property in originalValues.PropertyNames)
                {
                    var originalValue = originalValues[property]?.ToString();
                    var currentValue = currentValues[property]?.ToString();

                    if (originalValue != currentValue)
                    {
                        var history = new TickectsHistory();
                        history.Changed = DateTimeOffset.Now;
                        history.NewValue = GetValueFromKey(property, currentValue);
                        history.OldValue = GetValueFromKey(property, originalValue);
                        history.Property = property;
                        history.TicketsId = MyTicket.Id;
                        history.UsersId = User.Identity.GetUserId();
                        changes.Add(history);
                    }
                }
                db.TickectsHistories.AddRange(changes);
                db.SaveChanges();

                if (MyTicket.AssignedToUser.Roles.Any(a => a.RoleId == "cdea5771-00b7-4def-a089-8968abffef2f"))
                {
                    var personalEmailService = new PersonalEmailService();

                    var mailMessage = new MailMessage(
                        WebConfigurationManager.AppSettings["emailto"], MyTicket.AssignedToUser.Email

                        );
                    mailMessage.Body = "Your ticket has changed, please confirm it as soon as possible";
                    mailMessage.Subject = "Ticket status changed";
                    mailMessage.IsBodyHtml = true;
                    personalEmailService.Send(mailMessage);
                }
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
            return View("Edit", tickets);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ADEdit([Bind(Include = "Id,Title,Description,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,AssignedToUserId")] Tickets tickets)
        {
            if (ModelState.IsValid)
            {
                var changes = new List<TickectsHistory>();
                var MyTicket = db.Tickets.First(p => p.Id == tickets.Id);
                MyTicket.Id = tickets.Id;
                MyTicket.Title = tickets.Title;
                MyTicket.Description = tickets.Description;
                MyTicket.Updated = DateTimeOffset.Now;
                MyTicket.TicketTypeId = tickets.TicketTypeId;
                MyTicket.TicketPriorityId = tickets.TicketPriorityId;
                MyTicket.TicketStatusId = tickets.TicketStatusId;
                MyTicket.ProjectId = tickets.ProjectId;

                var originalValues = db.Entry(MyTicket).OriginalValues;
                var currentValues = db.Entry(MyTicket).CurrentValues;

                foreach (var property in originalValues.PropertyNames)
                {
                    var originalValue = originalValues[property]?.ToString();
                    var currentValue = currentValues[property]?.ToString();

                    if (originalValue != currentValue)
                    {
                        var history = new TickectsHistory();
                        history.Changed = DateTimeOffset.Now;
                        history.NewValue = GetValueFromKey(property, currentValue);
                        history.OldValue = GetValueFromKey(property, originalValue);
                        history.Property = property;
                        history.TicketsId = MyTicket.Id;
                        history.UsersId = User.Identity.GetUserId();
                        changes.Add(history);
                    }
                }
                db.TickectsHistories.AddRange(changes);

                db.SaveChanges();

                if (MyTicket.AssignedToUser.Roles.Any(a => a.RoleId == "cdea5771-00b7-4def-a089-8968abffef2f"))
                {
                    var personalEmailService = new PersonalEmailService();

                    var mailMessage = new MailMessage(
                        WebConfigurationManager.AppSettings["emailto"], MyTicket.AssignedToUser.Email

                        );
                    mailMessage.Body = "Your ticket has changed, please confirm it as soon as possible";
                    mailMessage.Subject = "Ticket status changed";
                    mailMessage.IsBodyHtml = true;
                    personalEmailService.Send(mailMessage);
                }
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
                var changes = new List<TickectsHistory>();
                var MyTicket = db.Tickets.First(p => p.Id == tickets.Id);
                MyTicket.Id = tickets.Id;
                MyTicket.Title = tickets.Title;
                MyTicket.Description = tickets.Description;
                MyTicket.Updated = DateTimeOffset.Now;

                var originalValues = db.Entry(MyTicket).OriginalValues;
                var currentValues = db.Entry(MyTicket).CurrentValues;

                foreach (var property in originalValues.PropertyNames)
                {
                    var originalValue = originalValues[property]?.ToString();
                    var currentValue = currentValues[property]?.ToString();

                    if (originalValue != currentValue)
                    {
                        var history = new TickectsHistory();
                        history.Changed = DateTimeOffset.Now;
                        history.NewValue = GetValueFromKey(property, currentValue);
                        history.OldValue = GetValueFromKey(property, originalValue);
                        history.Property = property;
                        history.TicketsId = MyTicket.Id;
                        history.UsersId = User.Identity.GetUserId();
                        changes.Add(history);
                    }
                }
                db.TickectsHistories.AddRange(changes);

                db.SaveChanges();
                return RedirectToAction("SubmitterIndex");
            }
            return View(tickets);
        }
        #endregion

        private string GetValueFromKey(string propertyName, string key)
        {
            if (propertyName == "TicketTypeId")
            {
                return db.TicketType.Find(Convert.ToInt32(key)).Name;
            }
            if (propertyName == "TicketPriorityId")
            {
                return db.TicketPriority.Find(Convert.ToInt32(key)).Name;
            }
            if (propertyName == "TicketStatusId")
            {
                return db.TicketStatus.Find(Convert.ToInt32(key)).Name;
            }
            if (propertyName == "ProjectId")
            {
                return db.Projects.Find(Convert.ToInt32(key)).Id.ToString();
            }
            return key;
        }

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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "ProjectManager")]
        public ActionResult PMAssign(int id)
        {
            var model = new ProjectsAssign();
            var aa = new IdentityRole();
            var devId = db.Roles.Where(p => p.Name == "Developer").Select(p => p.Id).FirstOrDefault();

            model.Id = id;
            var ticket = db.Tickets.FirstOrDefault(p => p.Id == id);
            var users = db.Users.ToList();
            var DelUsers = db.Users.Where(p => p.Roles.Any(q => q.RoleId == devId));

            var userIdsAssignedToProject = ticket.Users.Select(p => p.Id).ToList();

            model.UserList = new MultiSelectList(DelUsers, "Id", "DisplayName", userIdsAssignedToProject);

            return View("PMAssign", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
                    var personalEmailService = new PersonalEmailService();

                    var mailMessage = new MailMessage(
                        WebConfigurationManager.AppSettings["emailto"], user.Email

                        );
                    mailMessage.Body = "Please confirm your tickect as soon as possible";
                    mailMessage.Subject = "You have an assigned tickect";
                    mailMessage.IsBodyHtml = true;
                    personalEmailService.Send(mailMessage);

                }
            }

            db.SaveChanges();

            return RedirectToAction("AdminIndex");
        }

        [HttpPost]
        [Authorize(Roles = "ProjectManager")]
        public ActionResult PMAssign(ProjectsAssign model)
        {
            var ticket = db.Tickets.FirstOrDefault(p => p.Id == model.Id);
            var assignedUsers = ticket.Users.ToList();
            var devId = db.Roles.Where(p => p.Name == "Developer").Select(p => p.Id).FirstOrDefault();
            var delUsers = assignedUsers.Where(p => p.Roles.Any(r => r.RoleId == devId));


            foreach (var user in delUsers)
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
                    var personalEmailService = new PersonalEmailService();

                    var mailMessage = new MailMessage(
                        WebConfigurationManager.AppSettings["emailto"], user.Email

                        );
                    mailMessage.Body = "Please confirm your tickect as soon as possible";
                    mailMessage.Subject = "You have an assigned tickect";
                    mailMessage.IsBodyHtml = true;
                    personalEmailService.Send(mailMessage);

                }
            }

            db.SaveChanges();

            return RedirectToAction("PMIndex");
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

            var devId = db.Roles.Where(p => p.Name == "Developer").Select(p => p.Id).FirstOrDefault();
            if (comments.AssignedToUser != null && comments.AssignedToUser.Roles.Any(p => p.RoleId == devId))
            {
                var personalEmailService = new PersonalEmailService();

                var mailMessage = new MailMessage(
                    WebConfigurationManager.AppSettings["emailto"], comments.AssignedToUser.Email

                    );
                mailMessage.Body = "You have a comment, please confirm it as soon as possible";
                mailMessage.Subject = "New one comment";
                mailMessage.IsBodyHtml = true;
                personalEmailService.Send(mailMessage);
            }

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

            var devId = db.Roles.Where(p => p.Name == "Developer").Select(p => p.Id).FirstOrDefault();
            if (attachments.AssignedToUser != null && attachments.AssignedToUser.Roles.Any(p => p.RoleId == devId))
            {
                var personalEmailService = new PersonalEmailService();

                var mailMessage = new MailMessage(
                    WebConfigurationManager.AppSettings["emailto"], attachments.AssignedToUser.Email

                    );
                mailMessage.Body = "You have a attachment, please confirm it as soon as possible";
                mailMessage.Subject = "New one attachment";
                mailMessage.IsBodyHtml = true;
                personalEmailService.Send(mailMessage);
            }
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
