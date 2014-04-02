using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace SharpStar.Lib.Database
{
    public class SharpStarPermission
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Permission { get; set; }

        public bool Allowed { get; set; }


    }
}
