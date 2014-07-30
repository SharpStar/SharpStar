using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandPermissionAttribute : Attribute
    {

        public string Permission { get; set; }

        public bool Admin { get; set; }

        public CommandPermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public CommandPermissionAttribute(bool admin)
        {
            Admin = admin;
        }

    }
}
