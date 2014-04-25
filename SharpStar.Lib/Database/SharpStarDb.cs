using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using SharpStar.Lib.Security;
using SQLite;

namespace SharpStar.Lib.Database
{
    public class SharpStarDb : ISharpStarDb
    {

        public string DatabaseName { get; private set; }

        public SharpStarDb(string dbName)
        {
            DatabaseName = dbName;
        }

        public void CreateTables()
        {

            var conn = new SQLiteConnection(DatabaseName);

            conn.CreateTable<SharpStarUser>();
            conn.CreateTable<SharpStarPermission>();
            conn.CreateTable<SharpStarGroup>();
            conn.CreateTable<SharpStarGroupPermission>();

            conn.Close();
            conn.Dispose();

        }

        public bool AddUser(string username, string password, bool admin = false, int? groupId = null)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarUser>();

            SharpStarUser user = tbl.SingleOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {

                string salt = SharpStarSecurity.GenerateSalt();

                conn.Insert(new SharpStarUser { Username = username, Hash = SharpStarSecurity.GenerateHash(username, password, salt, 5000), Salt = salt, IsAdmin = admin, GroupId = groupId });

            }

            conn.Close();
            conn.Dispose();

            return user == null;

        }

        public bool ChangeUserGroup(string username, int newGroupId)
        {

            var usr = GetUser(username);

            if (usr == null)
                return false;

            return ChangeUserGroup(usr.Id, newGroupId);

        }

        public bool ChangeUserGroup(int userId, int newGroupId)
        {

            var usr = GetUser(userId);

            if (usr == null)
                return false;

            var conn = new SQLiteConnection(DatabaseName);

            usr.GroupId = newGroupId;

            conn.Update(usr);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public bool UpdateUserPassword(string username, string password)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarUser>();

            SharpStarUser user = tbl.SingleOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {

                conn.Close();
                conn.Dispose();

                return false;

            }

            string salt = SharpStarSecurity.GenerateSalt();

            user.Hash = SharpStarSecurity.GenerateHash(username, password, salt, 5000);
            user.Salt = salt;

            conn.Update(user);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public bool DeleteUser(string username)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarUser>();

            SharpStarUser user = tbl.SingleOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {

                conn.Close();
                conn.Dispose();

                return false;

            }

            var permTbl = conn.Table<SharpStarPermission>();
            var perms = permTbl.Where(p => p.UserId == user.Id);

            //cleanup
            foreach (var perm in perms)
            {
                conn.Delete<SharpStarPermission>(perm.Id);
            }

            conn.Delete<SharpStarUser>(user.Id);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public SharpStarUser GetUser(string username)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarUser>();

            SharpStarUser user = tbl.SingleOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            conn.Close();
            conn.Dispose();

            return user;

        }

        public SharpStarUser GetUser(int id)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarUser>();

            SharpStarUser user = tbl.SingleOrDefault(p => p.Id == id);

            conn.Close();
            conn.Dispose();

            return user;

        }

        public List<SharpStarUser> GetUsers()
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarUser>();

            List<SharpStarUser> users = tbl.ToList();

            conn.Close();
            conn.Dispose();

            return users;

        }

        public int GetUserCount()
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarUser>();

            int userCount = tbl.Count();

            conn.Close();
            conn.Dispose();

            return userCount;

        }

        public SharpStarPermission GetPlayerPermission(int userId, string permission)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarPermission>();

            var perm = tbl.SingleOrDefault(p => p.UserId == userId && p.Permission == permission);

            conn.Close();
            conn.Dispose();

            return perm;

        }

        public List<SharpStarPermission> GetPlayerPermissions(int userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarPermission>();

            var perms = tbl.Where(p => p.UserId == userId);

            conn.Close();
            conn.Dispose();

            return perms.ToList();

        }

        public void AddPlayerPermission(int userId, string permission, bool allowed)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarPermission>();

            var perm = tbl.SingleOrDefault(p => p.UserId == userId && p.Permission == permission);

            if (perm != null)
            {

                perm.Allowed = allowed;

                conn.Update(perm);

                return;

            }

            conn.Insert(new SharpStarPermission { UserId = userId, Permission = permission, Allowed = allowed });

            conn.Close();
            conn.Dispose();

        }

        public void DeletePlayerPermission(int userId, string permission)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarPermission>();

            var perm = tbl.SingleOrDefault(p => p.Permission == permission);

            if (perm == null)
                return;

            conn.Delete<SharpStarPermission>(perm.Id);

        }

        public void ChangeAdminStatus(int userId, bool admin)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var usr = GetUser(userId);

            usr.IsAdmin = admin;

            conn.Update(usr);

            conn.Close();
            conn.Dispose();

        }

        public SharpStarGroup CreateGroup(string groupName, bool defaultGroup = false)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarGroup>();

            if (tbl.Any(p => p.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase)))
            {

                conn.Close();
                conn.Dispose();

                return null;

            }

            if (defaultGroup)
            {

                var defGroup = tbl.SingleOrDefault(p => p.IsDefaultGroup);

                if (defGroup != null)
                {

                    defGroup.IsDefaultGroup = false;

                    conn.Update(defGroup);

                }

            }

            var group = new SharpStarGroup
            {
                GroupName = groupName,
                IsDefaultGroup = defaultGroup
            };

            int id = conn.Insert(group);

            group.Id = id;

            return group;

        }

        public bool DeleteGroup(string groupName)
        {

            var group = GetGroup(groupName);

            if (group == null)
                return false;

            return DeleteGroup(group.Id);

        }

        public bool DeleteGroup(int groupId)
        {

            var group = GetGroup(groupId);

            if (group == null)
                return false;

            var conn = new SQLiteConnection(DatabaseName);

            var usrTbl = conn.Table<SharpStarUser>();

            var users = usrTbl.Where(p => p.GroupId == groupId);

            //cleanup
            foreach (var user in users)
            {
                user.GroupId = null;
            }

            conn.UpdateAll(users);

            var permTbl = conn.Table<SharpStarGroupPermission>();

            var perms = permTbl.Where(p => p.GroupId == groupId);

            foreach (var perm in perms)
            {
                conn.Delete<SharpStarGroupPermission>(perm.Id);
            }

            conn.Delete(group);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public bool SetDefaultGroup(string groupName)
        {

            var group = GetGroup(groupName);

            if (group == null)
                return false;

            return SetDefaultGroup(group.Id);

        }

        public bool SetDefaultGroup(int groupId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarGroup>();

            var defGroup = tbl.SingleOrDefault(p => p.IsDefaultGroup);

            if (defGroup != null)
            {

                defGroup.IsDefaultGroup = false;

                conn.Update(defGroup);

            }

            var group = GetGroup(groupId);

            if (group == null)
                return false;

            group.IsDefaultGroup = true;

            conn.Update(group);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public SharpStarGroup GetGroup(string groupName)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarGroup>();

            var group = tbl.SingleOrDefault(p => p.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));

            conn.Close();
            conn.Dispose();

            return group;

        }

        public SharpStarGroup GetGroup(int groupId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var group = conn.Find<SharpStarGroup>(groupId);

            conn.Close();
            conn.Dispose();

            return group;

        }

        public bool AddGroupPermission(string groupName, string permission, bool allowed = true)
        {

            SharpStarGroup group = GetGroup(groupName);

            if (group == null)
                return false;

            return AddGroupPermission(group.Id, permission, allowed);

        }

        public bool AddGroupPermission(int groupId, string permission, bool allowed = true)
        {

            SharpStarGroup group = GetGroup(groupId);

            if (group == null)
                return false;

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarGroupPermission>();

            if (tbl.Any(p => p.GroupId == group.Id && p.Permission.Equals(permission, StringComparison.OrdinalIgnoreCase)))
            {

                conn.Close();
                conn.Dispose();

                return false;

            }

            var perm = new SharpStarGroupPermission
            {
                GroupId = group.Id,
                Permission = permission,
                Allowed = allowed
            };

            conn.Insert(perm);

            conn.Close();
            conn.Dispose();

            return true;

        }

        public List<SharpStarGroupPermission> GetGroupPermissions(int groupId)
        {

            SharpStarGroup group = GetGroup(groupId);

            if (group == null)
                return null;

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarGroupPermission>();

            var perms = tbl.Where(p => p.GroupId == groupId).ToList();

            conn.Close();
            conn.Dispose();

            return perms;

        }

        public SharpStarGroupPermission GetGroupPermission(int groupId, string permission)
        {


            SharpStarGroup group = GetGroup(groupId);

            if (group == null)
                return null;

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarGroupPermission>();

            SharpStarGroupPermission perm = tbl.SingleOrDefault(p => p.GroupId == groupId && p.Permission.Equals(permission, StringComparison.OrdinalIgnoreCase));

            conn.Close();
            conn.Dispose();

            return perm;

        }

        public bool RemoveGroupPermission(string groupName, string permission)
        {

            SharpStarGroup group = GetGroup(groupName);

            if (group == null)
                return false;

            return RemoveGroupPermission(group.Id, permission);

        }
        
        public bool RemoveGroupPermission(int groupId, string permission)
        {

            SharpStarGroup group = GetGroup(groupId);

            if (group == null)
                return false;

            var conn = new SQLiteConnection(DatabaseName);

            var perm = GetGroupPermission(groupId, permission);

            if (perm == null)
            {

                conn.Close();
                conn.Dispose();

                return false;

            }

            conn.Delete<SharpStarGroupPermission>(perm.Id);

            conn.Close();
            conn.Dispose();

            return true;

        }

    }
}
