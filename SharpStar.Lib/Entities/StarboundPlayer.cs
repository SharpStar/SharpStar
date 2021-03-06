﻿// SharpStar
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
using SharpStar.Lib.Database;
using SharpStar.Lib.DataTypes;

namespace SharpStar.Lib.Entities
{
    /// <summary>
    /// Represents a player in Starbound
    /// </summary>
    public class StarboundPlayer : IDisposable
    {

        /// <summary>
        /// The player's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the player including the color tags (if any)
        /// </summary>
        public string NameWithColor { get; set; }

        /// <summary>
        /// The player's UUID
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// The player's Entity id
        /// </summary>
        public long EntityId { get; set; }

        /// <summary>
        /// The player's species
        /// </summary>
        public string Species { get; set; }

        /// <summary>
        /// The client's Id
        /// </summary>
        public uint ClientId { get; set; }

        /// <summary>
        /// The user that the player is logged in as
        /// It is null when the player is not authenticated.
        /// </summary>
        public SharpStarUser UserAccount { get; set; }

        /// <summary>
        /// The player's coordinates
        /// </summary>

        public WorldCoordinate Coordinates { get; set; }

        /// <summary>
        /// The player's home coordinates
        /// </summary>
        public WorldCoordinate HomeCoordinates { get; set; }

        /// <summary>
        /// Spawn x coordinate
        /// </summary>
        public float SpawnX { get; set; }

        /// <summary>
        /// Spawn Y coordinate
        /// </summary>
        public float SpawnY { get; set; }

        /// <summary>
        /// True if the player is on a ship, false otherwise
        /// </summary>
        public bool OnShip { get; set; }

        /// <summary>
        /// True if the player is on their own ship, false otherwise
        /// </summary>
        public bool OnOwnShip { get; set; }

        /// <summary>
        /// The owner of the ship the player is currently on
        /// </summary>
        public string PlayerShip { get; set; }

        /// <summary>
        /// True if the player attempted to authenticate with SharpStar
        /// </summary>
        public bool AttemptedLogin { get; set; }

        /// <summary>
        /// True if logged in as guest, false if not
        /// </summary>
        public bool Guest { get; set; }

        /// <summary>
        /// True if the player joined successfully, false if not
        /// </summary>
        public bool JoinSuccessful { get; set; }

        public StarboundPlayer()
        {
        }

        public StarboundPlayer(string name, string uuid)
            : this()
        {
            Name = name;
            UUID = uuid;
        }

        /// <summary>
        /// Add a permission associated with this player's user account
        /// This method does nothing if the player is not authenticated with an account
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="allowed"></param>
        public void AddPermission(string permission, bool allowed)
        {
            if (UserAccount == null)
                return;

            SharpStarMain.Instance.Database.AddPlayerPermission(UserAccount.Id, permission, allowed);
        }

        /// <summary>
        /// Removes a permission associated with this player's user account
        /// This method does nothing if the player is not authenticated with an account
        /// </summary>
        /// <param name="permission"></param>
        public void RemovePermission(string permission)
        {

            if (UserAccount == null)
                return;

            SharpStarMain.Instance.Database.DeletePlayerPermission(UserAccount.Id, permission);

        }

        /// <summary>
        /// Determines whether or not this player has the specified permission
        /// </summary>
        /// <param name="permission">The permission to check</param>
        /// <returns>True if this player has the specified permission, false otherwise</returns>
        public bool HasPermission(string permission)
        {

            if (UserAccount == null)
                return false;

            if (UserAccount.IsAdmin)
                return true;

            bool groupAllowed = false;

            if (UserAccount.Group != null)
            {
                var groupPerm = SharpStarMain.Instance.Database.GetGroupPermission(UserAccount.Group.Id, permission);

                groupAllowed = groupPerm != null && groupPerm.Allowed;
            }

            if (groupAllowed)
                return true;

            var perm = SharpStarMain.Instance.Database.GetPlayerPermission(UserAccount.Id, permission);

            return perm != null && perm.Allowed;

        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            UserAccount = null;
            Coordinates = null;
            HomeCoordinates = null;
        }

        ~StarboundPlayer()
        {
            Dispose(false);
        }

    }
}
