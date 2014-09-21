using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace SharpStar.Lib.Database.Mappings
{
    public class GroupPermissionMap : ClassMap<SharpStarGroupPermission>
    {
        public GroupPermissionMap()
        {
            Id(m => m.Id);
            References(m => m.Group).LazyLoad().Column("GroupId").Not.Nullable();
            Map(m => m.Permission);
            Map(m => m.Allowed);
        }
    }
}
