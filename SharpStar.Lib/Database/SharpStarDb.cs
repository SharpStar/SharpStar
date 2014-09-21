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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SQLite;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using SharpStar.Lib.Database.Mono;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Misc;
using SharpStar.Lib.Mono;
using SharpStar.Lib.Security;

namespace SharpStar.Lib.Database
{
    public class SharpStarDb
    {

        public string DatabaseName { get; private set; }

        private readonly ISessionFactory Factory;

        private Configuration _config;

        public SharpStarDb(string dbName)
        {
            DatabaseName = dbName;
            Factory = GetSessionFactory();
        }

        public ISessionFactory GetSessionFactory()
        {
            var config = Fluently.Configure();

            if (!File.Exists(DatabaseName))
            {
                config = config.ExposeConfiguration(p => new SchemaExport(p).Execute(false, true, false));
            }

            if (MonoHelper.IsRunningOnMono())
            {
                config = config.Database(MonoSQLiteConfiguration.Standard.UsingFile(DatabaseName));
            }
            else
            {
                config = config.Database(SQLiteConfiguration.Standard.UsingFile(DatabaseName));
            }
            _config = config
              .Mappings(p => p.FluentMappings.AddFromAssemblyOf<SharpStarUser>())
              .BuildConfiguration();

            MigrateToLatest();

            return config.BuildSessionFactory();
        }


        public ISession CreateSession()
        {
            return Factory.OpenSession();
        }

        public class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; set; }
            public string ProviderSwitches { get; set; }
            public int Timeout { get; set; }
        }

        public void MigrateToLatest()
        {
            var announcer = new TextWriterAnnouncer(s => SharpStarLogger.DefaultLogger.Debug(s));
            var assembly = Assembly.GetExecutingAssembly();

            var migrationContext = new RunnerContext(announcer)
            {
                Namespace = "SharpStar.Database.Migrations"
            };

            var options = new MigrationOptions { PreviewOnly = false, Timeout = 60 };

            ReflectionBasedDbFactory factory;

            if (MonoHelper.IsRunningOnMono())
                factory = new MonoSQLiteDbFactory();
            else
                factory = new SqliteDbFactory();

            var connection = factory.CreateConnection(_config.GetProperty(NHibernate.Cfg.Environment.ConnectionString));

            var processor = new SqliteProcessor(connection, new SqliteGenerator(), announcer, options, factory);
            var runner = new MigrationRunner(assembly, migrationContext, processor);
            runner.MigrateUp(true);
        }

        public bool AddUser(string username, string password, bool admin = false, int? groupId = null)
        {

            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    SharpStarUser user = session.Query<SharpStarUser>().SingleOrDefault(p => p.Username.ToLower() == username.ToLower());

                    if (user == null)
                    {
                        string salt = SharpStarSecurity.GenerateSalt();

                        SharpStarGroup group = null;
                        if (!groupId.HasValue)
                        {
                            SharpStarGroup defaultGroup = session.Query<SharpStarGroup>().SingleOrDefault(p => p.IsDefaultGroup);

                            if (defaultGroup != null)
                            {
                                group = defaultGroup;
                            }
                        }
                        else
                        {
                            group = session.Query<SharpStarGroup>().SingleOrDefault();
                        }

                        session.Save(new SharpStarUser { Username = username, Hash = SharpStarSecurity.GenerateHash(username, password, salt, 5000), Salt = salt, IsAdmin = admin, Group = group });

                        transaction.Commit();
                    }

                    return user == null;
                }
            }
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

            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    usr.Group = GetGroup(newGroupId);

                    session.SaveOrUpdate(usr);

                    transaction.Commit();
                }
            }

            return true;
        }

        public bool UpdateUserLastLogin(string username, DateTime lastLogin)
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    SharpStarUser user = session.Query<SharpStarUser>().SingleOrDefault(p => p.Username.ToLower() == username.ToLower());

                    if (user == null)
                    {
                        return false;
                    }

                    user.LastLogin = lastLogin;

                    session.SaveOrUpdate(user);

                    transaction.Commit();
                }

            }

            return true;
        }

        public bool UpdateUserPassword(string username, string password)
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    SharpStarUser user = session.Query<SharpStarUser>().SingleOrDefault(p => p.Username.ToLower() == username.ToLower());

                    if (user == null)
                    {
                        return false;
                    }

                    string salt = SharpStarSecurity.GenerateSalt();

                    user.Hash = SharpStarSecurity.GenerateHash(username, password, salt, StarboundConstants.Rounds);
                    user.Salt = salt;

                    session.SaveOrUpdate(user);

                    transaction.Commit();
                }
            }

            return true;
        }

        public bool DeleteUser(string username)
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    SharpStarUser user = session.Query<SharpStarUser>().SingleOrDefault(p => p.Username.ToLower() == username.ToLower());

                    if (user == null)
                    {
                        return false;
                    }

                    session.Delete(user);

                    transaction.Commit();
                }
            }

            return true;
        }

        public SharpStarUser GetUser(string username)
        {
            using (var session = CreateSession())
            {
                return session.Query<SharpStarUser>().SingleOrDefault(p => p.Username.ToLower() == username.ToLower());
            }
        }

        public SharpStarUser GetUser(int id)
        {
            using (var session = CreateSession())
            {
                return session.Get<SharpStarUser>(id);
            }
        }

        public List<SharpStarUser> GetUsers()
        {
            using (var session = CreateSession())
            {
                return session.CreateCriteria<SharpStarUser>().List<SharpStarUser>().ToList();
            }
        }

        public int GetUserCount()
        {
            using (var session = CreateSession())
            {
                return session.Query<SharpStarUser>().Count();
            }
        }

        public SharpStarPermission GetPlayerPermission(int userId, string permission)
        {
            using (var session = CreateSession())
            {
                return session.Query<SharpStarPermission>().SingleOrDefault(p => p.User.Id == userId && p.Permission == permission);
            }
        }

        public List<SharpStarPermission> GetPlayerPermissions(int userId)
        {
            using (var session = CreateSession())
            {
                return session.Query<SharpStarPermission>().Where(p => p.User.Id == userId).ToList();
            }
        }

        public void AddPlayerPermission(int userId, string permission, bool allowed)
        {

            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    SharpStarPermission perm = session.Query<SharpStarPermission>().SingleOrDefault(p => p.User.Id == userId && p.Permission == permission);

                    if (perm != null)
                    {
                        perm.Allowed = allowed;

                        session.SaveOrUpdate(perm);
                    }
                    else
                    {
                        session.Save(new SharpStarPermission { User = GetUser(userId), Permission = permission, Allowed = allowed });
                    }

                    transaction.Commit();
                }
            }
        }

        public void DeletePlayerPermission(int userId, string permission)
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    SharpStarPermission perm = session.Query<SharpStarPermission>().SingleOrDefault(p => p.User.Id == userId && p.Permission == permission);

                    if (perm != null)
                    {
                        session.Delete(perm);
                    }

                    transaction.Commit();
                }
            }
        }

        public void ChangeAdminStatus(int userId, bool admin)
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    SharpStarUser user = GetUser(userId);

                    if (user != null)
                    {
                        user.IsAdmin = admin;

                        session.SaveOrUpdate(user);

                        transaction.Commit();
                    }
                }

            }
        }

        public SharpStarGroup CreateGroup(string groupName, bool defaultGroup = false)
        {

            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    if (session.Query<SharpStarGroup>().Any(p => p.GroupName.ToLower() == groupName.ToLower()))
                    {
                        return null;
                    }

                    if (defaultGroup)
                    {
                        var defGroup = session.Query<SharpStarGroup>().SingleOrDefault(p => p.IsDefaultGroup);

                        if (defGroup != null)
                        {
                            defGroup.IsDefaultGroup = false;

                            session.SaveOrUpdate(defGroup);
                        }
                    }

                    var group = new SharpStarGroup
                    {
                        GroupName = groupName,
                        IsDefaultGroup = defaultGroup
                    };

                    session.SaveOrUpdate(group);

                    transaction.Commit();

                    return group;

                }
            }
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

            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var users = session.Query<SharpStarGroup>().Select(p => p.Users);

                    foreach (SharpStarUser user in users)
                    {
                        user.Group = null;

                        session.SaveOrUpdate(user);
                    }

                    session.Delete(group);

                    transaction.Commit();
                }
            }

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
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    SharpStarGroup defGroup = session.Query<SharpStarGroup>().SingleOrDefault(p => p.IsDefaultGroup);

                    if (defGroup != null)
                    {
                        defGroup.IsDefaultGroup = false;

                        session.SaveOrUpdate(defGroup);
                    }

                    SharpStarGroup group = GetGroup(groupId);

                    if (group != null)
                    {
                        group.IsDefaultGroup = true;

                        session.SaveOrUpdate(group);

                        transaction.Commit();

                        return true;
                    }

                }
            }

            return false;
        }

        public SharpStarGroup GetGroup(string groupName)
        {
            using (var session = CreateSession())
            {
                return session.Query<SharpStarGroup>().SingleOrDefault(p => p.GroupName.ToLower() == groupName.ToLower());
            }
        }

        public SharpStarGroup GetGroup(int groupId)
        {
            using (var session = CreateSession())
            {
                return session.Get<SharpStarGroup>(groupId);
            }
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

            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    if (session.Query<SharpStarGroupPermission>().Any(p => p.Group.Id == group.Id && p.Permission.ToLower() == permission.ToLower()))
                        return false;

                    var perm = new SharpStarGroupPermission
                    {
                        Group = group,
                        Permission = permission,
                        Allowed = allowed
                    };

                    session.Save(perm);

                    transaction.Commit();
                }
            }

            return true;
        }

        public List<SharpStarGroupPermission> GetGroupPermissions(int groupId)
        {
            using (var session = CreateSession())
            {
                SharpStarGroup group = session.Get<SharpStarGroup>(groupId);

                if (group == null)
                    return null;

                return group.Permissions.ToList();
            }
        }

        public SharpStarGroupPermission GetGroupPermission(int groupId, string permission)
        {
            using (var session = CreateSession())
            {
                return session.Query<SharpStarGroupPermission>().SingleOrDefault(p => p.Group.Id == groupId && p.Permission.ToLower() == permission.ToLower());
            }
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
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    SharpStarGroupPermission perm = session.Query<SharpStarGroupPermission>().SingleOrDefault(p => p.Group.Id == groupId && p.Permission.ToLower() == permission.ToLower());

                    if (perm == null)
                        return false;

                    session.Delete(perm);

                    transaction.Commit();

                    return true;
                }
            }
        }

    }
}
