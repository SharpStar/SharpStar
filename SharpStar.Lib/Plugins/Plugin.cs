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
using SharpStar.Lib.Server;

namespace SharpStar.Lib.Plugins
{
    public abstract class Plugin : IPlugin
    {
        public abstract string PluginFile { get; protected set; }
        public abstract bool Enabled { get; protected set; }

        public virtual bool PassChatCommand(string command, IClient client, string[] args)
        {
            return false;
        }

        public virtual bool PassConsoleCommand(string command, string[] args)
        {
            return false;
        }

        public virtual void OnLoad()
        {
        }

        public virtual void OnUnload()
        {
        }

        public virtual void CallEvent(string evtName, params object[] args)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}