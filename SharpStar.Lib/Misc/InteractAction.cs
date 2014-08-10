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
using SharpStar.Lib.Networking;
using SharpStar.Lib.DataTypes;

namespace SharpStar.Lib.Misc
{
    public class InteractAction : IWriteable
    {

        public InteractActionType ActionType { get; set; }

        public int EntityId { get; set; }

        public Variant Data { get; set; }

        public static InteractAction FromStream(IStarboundStream stream)
        {
            InteractAction action = new InteractAction();
            action.ActionType = (InteractActionType)stream.ReadUInt8();
            action.EntityId = stream.ReadInt32();
            action.Data = stream.ReadVariant();

            return action;
        }

        public void WriteTo(IStarboundStream stream)
        {
            stream.WriteUInt8((byte)ActionType);
            stream.WriteInt32(EntityId);
            stream.WriteVariant(Data);
        }
    }

    public enum InteractActionType : byte
    {
        None = 0,
        OpenCockpitInterface = 1,
        OpenContainer = 2,
        SitDown = 3,
        OpenCraftingInterface = 4,
        OpenCookingInterface = 5,
        OpenTechInterface = 6,
        Teleport = 7,
        PlayCinematic = 8,
        OpenSongbookInterface = 9,
        OpenNpcInterface = 10,
        OpenNpcCraftingInterface = 11,
        OpenTech3DPrinterDialog = 12,
        OpenTeleportDialog = 13,
        ShowPopup = 14
    }
}
