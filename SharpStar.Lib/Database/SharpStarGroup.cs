using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace SharpStar.Lib.Database
{
    public class SharpStarGroup
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string GroupName { get; set; }

        public bool IsDefaultGroup { get; set; }

    }
}
