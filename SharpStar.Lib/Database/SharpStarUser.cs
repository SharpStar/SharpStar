using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace SharpStar.Lib.Database
{
    public class SharpStarUser
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Username { get; set; }

        public string Hash { get; set; }

        public string Salt { get; set; }

        public DateTime LastLogin { get; set; }

    }
}
