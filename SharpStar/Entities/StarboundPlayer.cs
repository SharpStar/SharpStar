using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Entities
{
    public class StarboundPlayer : IEntity
    {

        public string Name { get; private set; }

        public Guid UUID { get; set; }

        public StarboundPlayer(string name, Guid uuid)
        {
            Name = name;
            UUID = uuid;
        }

    }
}
