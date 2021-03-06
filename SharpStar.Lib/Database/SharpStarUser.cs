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
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Database
{
    public class SharpStarUser
    {

        public virtual int Id { get; set; }

        public virtual string Username { get; set; }

        public virtual string Hash { get; set; }

        public virtual string Salt { get; set; }

        public virtual bool IsAdmin { get; set; }

        public virtual SharpStarGroup Group { get; set; }

        public virtual DateTime LastLogin { get; set; }

        public virtual IList<SharpStarPermission> Permissions { get; set; }

    }
}
