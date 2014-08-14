using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Networking;
using SharpStar.Lib.DataTypes;

namespace SharpStar.Lib.Entities
{
    public class SpawnedProjectile : SpawnedEntity
    {

        public string ProjectileKey { get; set; }

        public Variant Parameters { get; set; }

    }
}
