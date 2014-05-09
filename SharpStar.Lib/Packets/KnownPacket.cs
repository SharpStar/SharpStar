using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Packets
{
    public enum KnownPacket : byte
    {
        ProtocolVersion = 0,
        ConnectionResponse = 1,
        DisconnectResponse = 2,
        HandshakeChallenge = 3,
        ChatReceived = 4,
        UniverseTimeUpdate = 5,
        CelestialResponse = 6,
        ClientConnect = 7,
        ClientDisconnect = 8,
        HandshakeResponse = 9,
        WarpCommand = 10,
        ChatSent = 11,
        CelestialRequest = 12,
        ClientContextUpdate = 13,
        WorldStart = 14,
        WorldStop = 15,
        TileArrayUpdate = 16,
        TileUpdate = 17,
        TileLiquidUpdate = 18,
        TileDamageUpdate = 19,
        TileModificationFailure = 20,
        GiveItem = 21,
        SwapContainerResult = 22,
        EnvironmentUpdate = 23,
        EntityInteractResult = 24,
        ModifyTileList = 25,
        DamageTile = 26,
        DamageTileGroup = 27,
        RequestDrop = 28,
        SpawnEntity = 29,
        EntityInteract = 30,
        ConnectWire = 31,
        DisconnectAllWires = 32,
        OpenContainer = 33,
        CloseContainer = 34,
        SwapContainer = 35,
        ItemApplyContainer = 36,
        StartCraftingContainer = 37,
        StopCraftingContainer = 38,
        BurnContainer = 39,
        ClearContainer = 40,
        WorldClientStateUpdate = 41,
        EntityCreate = 42,
        EntityUpdate = 43,
        EntityDestroy = 44,
        DamageNotification = 45,
        StatusEffectRequest = 46,
        UpdateWorldProperties = 47,
        Heartbeat = 48
    }
}
