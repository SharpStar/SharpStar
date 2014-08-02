using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Tiles
{
    public class TileDamageParameters : IWriteable
    {

        public Variant DamageFactors { get; set; }

        public float TotalHealth { get; set; }

        public static TileDamageParameters FromStream(IStarboundStream stream)
        {
            TileDamageParameters parameters = new TileDamageParameters();
            parameters.DamageFactors = stream.ReadVariant();
            parameters.TotalHealth = stream.ReadSingle();

            return parameters;
        }

        public void WriteTo(IStarboundStream stream)
        {
            stream.WriteVariant(DamageFactors);
            stream.WriteSingle(TotalHealth);
        }
    }
}
