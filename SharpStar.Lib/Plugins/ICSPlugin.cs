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
using System.Threading.Tasks;
using Mono.Addins;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    [TypeExtensionPoint]
    public interface ICSPlugin
    {

        string Name { get; }

        void OnLoad();

        void OnUnload();

        void OnPluginLoaded(ICSPlugin plugin);

        void OnPluginUnloaded(ICSPlugin plugin);

        Task OnEventOccurred(IPacket packet, SharpStarClient client, bool isAfter = false);

        Task<bool> OnChatCommandReceived(SharpStarClient client, string command, string[] args);

        bool OnConsoleCommand(string command, string[] args);

    }
}
