﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_BugTraker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        public int TicketsId { get; set; }
        public virtual Tickets Tickets { get; set; }
        public string MediaURL { get; set; }
        public DateTimeOffset Created { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }

        public string UsersId { get; set; }
        public virtual ApplicationUser Users { get; set; }
        
    }
}