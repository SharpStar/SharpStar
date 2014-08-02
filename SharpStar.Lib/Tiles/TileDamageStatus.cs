using SharpStar.Lib.DataTypes;
using SharpStar.Lib.Networking;

namespace SharpStar.Lib.Tiles
{
    public class TileDamageStatus : IWriteable
    {

        public TileDamageParameters Parameters { get; set; }

        public Vec2F SourcePosition { get; set; }

        public TileDamage Damage { get; set; }

        public static TileDamageStatus FromStream(IStarboundStream stream)
        {
            TileDamageStatus status = new TileDamageStatus();
            status.Parameters = TileDamageParameters.FromStream(stream);
            status.SourcePosition = Vec2F.FromStream(stream);
            status.Damage = TileDamage.FromStream(stream);

            return status;
        }

        public void WriteTo(IStarboundStream stream)
        {
            Parameters.WriteTo(stream);
            SourcePosition.WriteTo(stream);
            Damage.WriteTo(stream);
        }

    }
}
