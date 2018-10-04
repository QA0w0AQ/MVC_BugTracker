using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_BugTraker.Models
{
    public class UserRoles
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public MultiSelectList Roles { get; set; }

        public string[] SelectedRoles { get; set; }
    }
}