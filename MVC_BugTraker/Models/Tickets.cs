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
        public virtual Projects Project { get; set; }

        public int TicketTypeId { get; set; }
        public virtual TicketType TicketType { get; set; }

        public int TicketPriorityId { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }

        public int TicketStatusId { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }

        public string OwnerUserId { get; set; }
        public virtual ApplicationUser OwnerUser { get; set; }

        public string AssignedToUserId { get; set; }
        public virtual ApplicationUser AssignedToUser { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<TicketAttachment> Attachments { get; set; }
        public virtual ICollection<TicketsComment> TicketsComments { get; set; }
        public virtual ICollection<TickectsHistory> TickectsHistories { get; set; }

        public Tickets()
        {
            Attachments = new HashSet<TicketAttachment>();
            Users = new HashSet<ApplicationUser>();
            TicketsComments = new HashSet<TicketsComment>();
            TickectsHistories = new HashSet<TickectsHistory>();
        }

    }
}