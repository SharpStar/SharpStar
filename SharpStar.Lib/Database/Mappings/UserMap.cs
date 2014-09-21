using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace SharpStar.Lib.Database.Mappings
{
    public class UserMap : ClassMap<SharpStarUser>
    {
        public UserMap()
        {
            Id(m => m.Id);
            Map(m => m.Username);
            Map(m => m.Hash);
            Map(m => m.Salt);
            Map(m => m.IsAdmin);
            References(m => m.Group).Nullable().LazyLoad().Column("GroupId");
            Map(m => m.LastLogin);
            HasMany(m => m.Permissions).LazyLoad().Cascade.AllDeleteOrphan().KeyColumn("UserId").Inverse();
        }
    }
}
