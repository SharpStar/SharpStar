// SharpStar
// Copyright (C) 2014 Mitchell Kutchuk
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
