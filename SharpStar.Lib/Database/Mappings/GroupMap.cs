using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace SharpStar.Lib.Database.Mappings
{
    public class GroupMap : ClassMap<SharpStarGroup>
    {
        public GroupMap()
        {
            Id(m => m.Id);
            Map(m => m.GroupName);
            Map(m => m.IsDefaultGroup);
            HasMany(m => m.Users).LazyLoad().Cascade.All().Inverse().KeyColumn("GroupId");
            HasMany(m => m.Permissions).LazyLoad().Cascade.AllDeleteOrphan().KeyColumn("GroupId").Inverse();
        }
    }
}
