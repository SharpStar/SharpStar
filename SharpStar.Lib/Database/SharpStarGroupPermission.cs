using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace SharpStar.Lib.Database
{
    public class SharpStarGroupPermission
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int GroupId { get; set; }

        public string Permission { get; set; }

        public bool Allowed { get; set; }

    }
}
