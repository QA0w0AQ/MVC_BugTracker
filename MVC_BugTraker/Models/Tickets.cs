using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_BugTraker.Models
{
    public class Tickets
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public int ProjectId { get; set; }
        public int TicketTyeId { get; set; }
        public int TicketPriorityId { get; set; }
        public int TicketStatusId { get; set; }
        public int OwnerUserId { get; set; }
        public int AssignedToUserId { get; set; }

        public virtual Projects GetProjects { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}