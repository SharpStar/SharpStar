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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharpStar.Lib.Config
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SharpStarConfigFile
    {
        [JsonProperty("listenPort")]
        public int ListenPort { get; set; }

        [JsonProperty("serverPort")]
        public int ServerPort { get; set; }

        [JsonProperty("PythonLibLocation")]
        public string PythonLibLocation { get; set; }

        [JsonProperty("enableAccounts")]
        public bool EnableAccounts { get; set; }

        [JsonProperty("requireAccountLogin")]
        public bool RequireAccountLogin { get; set; }

        [JsonProperty("requireAccountLoginError")]
        public string RequireAccountLoginError { get; set; }

        [JsonProperty("maxPlayers")]
        public int MaxPlayers { get; set; }

        [JsonProperty("maxPlayersRejectReason")]
        public string MaxPlayerRejectionReason { get; set; }

        [JsonProperty("showDebug")]
        public bool ShowDebug { get; set; }

        [JsonProperty("autoUpdatePlugins")]
        public bool AutoUpdatePlugins { get; set; }

        [JsonProperty("starboundBind")]
        public string StarboundBind { get; set; }

        [JsonProperty("sharpstarBind")]
        public string SharpStarBind { get; set; }

        [JsonProperty("allowedStarboundCommands")]
        public List<string> AllowedStarboundCommands { get; set; }

        [JsonProperty("serverOfflineError")]
        public string ServerOfflineError { get; set; }

        public SharpStarConfigFile()
        {
            EnableAccounts = true;
            RequireAccountLogin = false;
            RequireAccountLoginError = "You must have a registered account in order to access this server!";
            ShowDebug = false;
            MaxPlayers = 100;
            MaxPlayerRejectionReason = "This server has reached the maximum player limit! Please try again later.";
            AutoUpdatePlugins = true;
            StarboundBind = null;
            SharpStarBind = "*";
            AllowedStarboundCommands = new List<string>();
            ServerOfflineError = "This server is currently offline! Please try again later.";
        }

    }
}