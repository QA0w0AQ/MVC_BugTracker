using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_BugTraker.Models
{
    public class DashBaord
    {
        public int TotallUsers { get; set; }
        public int TotallProjects { get; set; }
        public int TotallTickets { get; set; }
        public int TotallComments { get; set; }
        public int AssignedTickets { get; set; }
        public int UserTickets { get; set; }
    }
}