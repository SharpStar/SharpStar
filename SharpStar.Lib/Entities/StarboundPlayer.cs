using SharpStar.Lib.Database;
using SharpStar.Lib.DataTypes;

namespace SharpStar.Lib.Entities
{
    public class StarboundPlayer
    {

        public string Name { get; private set; }

        public string UUID { get; set; }

        public long EntityId { get; set; }

        public SharpStarUser UserAccount { get; set; }

        public WorldCoordinate Coordinates { get; set; }

        public WorldCoordinate HomeCoordinates { get; set; }

        public float SpawnX { get; set; }

        public float SpawnY { get; set; }

        public bool OnShip { get; set; }

        public bool AttemptedLogin { get; set; }

        public StarboundPlayer()
        {
        }

        public StarboundPlayer(string name, string uuid)
        {
            Name = name;
            UUID = uuid;
        }

        public void AddPermission(string permission, bool allowed)
        {

            if (UserAccount == null)
                return;

            SharpStarMain.Instance.Database.AddPlayerPermission(UserAccount.Id, permission, allowed);

        }

        public void RemovePermission(string permission)
        {

            if (UserAccount == null)
                return;

            SharpStarMain.Instance.Database.DeletePlayerPermission(UserAccount.Id, permission);

        }

        public bool HasPermission(string permission)
        {

            if (UserAccount == null)
                return false;

            var perm = SharpStarMain.Instance.Database.GetPlayerPermission(UserAccount.Id, permission);

            return perm != null && perm.Allowed;

        }

    }
}
