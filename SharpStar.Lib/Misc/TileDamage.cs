using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Misc
{
    public class TileDamage
    {

        public TileDamageType DamageType { get; set; }

        public float Amount { get; set; }

        public static TileDamage FromStream(IStarboundStream stream)
        {
            TileDamage dmg = new TileDamage();
            dmg.DamageType = (TileDamageType)stream.ReadUInt8();
            dmg.Amount = stream.ReadSingle();

            return dmg;
        }

        public void WriteTo(IStarboundStream stream)
        {
            stream.WriteUInt8((byte)DamageType);
            stream.WriteSingle(Amount);
        }

    }

    public enum TileDamageType : byte
    {
        None = 0,
        Plantish = 1,
        Blockish = 2,
        Beamish = 3,
        Explosive = 4,
        Fire = 5,
        Tilling = 6,
        Crushing = 7
    }

}
