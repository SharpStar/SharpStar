using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace SharpStar.Lib.Database.Mappings
{
    public class PermissionMap : ClassMap<SharpStarPermission>
    {
        public PermissionMap()
        {
            Id(m => m.Id);
            References(m => m.User).LazyLoad().Not.Nullable().Column("UserId");
            Map(m => m.Permission);
            Map(m => m.Allowed);
        }
    }
}
