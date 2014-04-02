using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using SharpStar.Lib.Security;
using SQLite;

namespace SharpStar.Lib.Database
{
    public class SharpStarDb
    {

        public string FileName { get; private set; }

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

            conn.Close();
            conn.Dispose();

        }

        public bool AddUser(string username, string password)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarUser>();

            SharpStarUser user = tbl.SingleOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {

                string salt = SharpStarSecurity.GenerateSalt();

                conn.Insert(new SharpStarUser { Username = username, Hash = SharpStarSecurity.GenerateHash(username, password, salt, 5000), Salt = salt });
            
            }

            conn.Close();
            conn.Dispose();

            return user == null;

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

            conn.Delete<SharpStarUser>(user.Id);

            //cleanup
            DeletePlayerPermissions(user.Id);

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

        public List<SharpStarUser> GetUsers()
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarUser>();

            List<SharpStarUser> users = tbl.ToList();

            conn.Close();
            conn.Dispose();

            return users;

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

        public void DeletePlayerPermissions(int userId)
        {

            var conn = new SQLiteConnection(DatabaseName);

            var tbl = conn.Table<SharpStarPermission>();

            var perms = tbl.Where(p => p.UserId == userId);

            foreach (var perm in perms)
            {
                conn.Delete<SharpStarPermission>(perm.Id);
            }

            conn.Close();
            conn.Dispose();

        }

    }
}
