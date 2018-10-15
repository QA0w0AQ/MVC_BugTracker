using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_BugTraker.Models
{
    public class TickectsHistory
    {
        public int Id { get; set; }

        public int TicketsId { get; set; }
        public virtual Tickets Tickets { get; set; }

        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public DateTimeOffset Changed { get; set; }

        public int UsersId { get; set; }
        public virtual ApplicationUser Users { get; set; }
    }
}