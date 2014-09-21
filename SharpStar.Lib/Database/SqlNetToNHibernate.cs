using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Linq;
using SQLite;

namespace SharpStar.Lib.Database
{
    public class SqlNetToNHibernate
    {

        public static void Migrate(string oldDb, string newDb)
        {
            using (var session = SharpStarMain.Instance.Database.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var conn = new SQLiteConnection(oldDb);

                    var usrTbl = conn.Table<OldDb.SharpStarUser>();
                    var grpTbl = conn.Table<OldDb.SharpStarGroup>();
                    var usrPermTbl = conn.Table<OldDb.SharpStarPermission>();
                    var grpPermTbl = conn.Table<OldDb.SharpStarGroupPermission>();

                    foreach (var usr in usrTbl)
                    {
                        SharpStarUser newUsr = new SharpStarUser
                        {
                            Username = usr.Username,
                            Salt = usr.Salt,
                            Hash = usr.Hash,
                            IsAdmin = usr.IsAdmin,
                            LastLogin = usr.LastLogin
                        };

                        session.Save(newUsr);

                        int usrId = usr.Id;
                        foreach (var usrPerm in usrPermTbl.Where(p => p.UserId == usrId))
                        {
                            session.Save(new SharpStarPermission
                            {
                                Permission = usrPerm.Permission,
                                Allowed = usrPerm.Allowed,
                                User = newUsr
                            });
                        }
                    }

                    foreach (var grp in grpTbl)
                    {
                        var newGrp = new SharpStarGroup
                        {
                            GroupName = grp.GroupName,
                            IsDefaultGroup = grp.IsDefaultGroup
                        };

                        session.Save(newGrp);

                        int grpId = grp.Id;
                        foreach (var usr in usrTbl.Where(p => p.GroupId == grpId))
                        {
                            string usrName = usr.Username;
                            var newUsr = session.Query<SharpStarUser>().SingleOrDefault(p => p.Username == usrName);

                            if (newUsr != null)
                            {
                                newUsr.Group = newGrp;
                            }
                        }

                        foreach (var grpPerm in grpPermTbl.Where(p => p.GroupId == grpId))
                        {
                            session.Save(new SharpStarGroupPermission
                            {
                                Permission = grpPerm.Permission,
                                Allowed = grpPerm.Allowed,
                                Group = newGrp
                            });
                        }

                    }

                    transaction.Commit();

                    conn.Close();
                    conn.Dispose();
                }
            }
        }

    }
}
