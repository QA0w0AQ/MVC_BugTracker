﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_BugTraker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Tickets> Tickets { get; set; }

        public TicketPriority()
        {
            Tickets = new HashSet<Tickets>();
        }
    }
}