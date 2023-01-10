namespace Tools
{

    public class NetworkDelegate
    {
        public delegate void OnServerUpdate();
        public delegate void OnUpdated();
    }
    
    public class Enums
    {
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