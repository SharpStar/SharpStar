using SharpStar.Lib.Entities;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Packets.Handlers
{
    public class EntityCreatePacketHandler : PacketHandler<EntityCreatePacket>
    {
        public override void Handle(EntityCreatePacket packet, StarboundClient client)
        {

            foreach (var ent in packet.Entities)
            {

                if (ent.EntityType == EntityType.Player && ent is PlayerEntity)
                {

                    PlayerEntity pent = (PlayerEntity)ent;

                    if (pent.UUID == client.Server.Player.UUID)
                    {
                        client.Server.Player.EntityId = ent.EntityId;
                    }

                }

            }

            SharpStarMain.Instance.PluginManager.CallEvent("entityCreate", packet, client);
        
        }

        public override void HandleAfter(EntityCreatePacket packet, StarboundClient client)
        {
            SharpStarMain.Instance.PluginManager.CallEvent("afterEntityCreate", packet, client);
        }
    }
}