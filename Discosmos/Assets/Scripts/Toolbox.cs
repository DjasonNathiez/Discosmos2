namespace Tools
{

    public class UtilityDelegate
    {
        public delegate void OnCooldown();
    }
    
    public class NetworkDelegate
    {
        public delegate void OnServerUpdate();
        public delegate void OnUpdated();
    }

    public class RaiseEvent
    {
        //PlayerSetup
        public static byte SetCharacter = 1;
        public static byte SetTeam = 2;
        
        //Input
        public static byte Input = 10;

        //BEGIN AT 100
        public static byte DamageTarget = 100;
        public static byte HealTarget = 101;
        public static byte Death = 102;
        public static byte HitStopTarget = 103;
        public static byte KnockBackTarget = 104;
    }
    
    public class Enums
    {
        public enum GameState
        {
            Disconnected,
            Hub,
            Game
        }

        public enum NetworkRoomState
        {
            Inside,
            Outside,
            Switch
        }
        
        public enum Characters
        {
            Mimi,
            Vega,
            Null
        }

        public enum Team
        {
            Pink,
            Green,
            Neutral
        }
        
        public enum MovementType
        {
            MoveToClick,
            KeepDirection,
            Slide,
            FollowTarget,
            Attack
        }
    }
}