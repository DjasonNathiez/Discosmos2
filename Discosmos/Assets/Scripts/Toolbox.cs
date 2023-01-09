namespace Tools
{

    public class NetworkDelegate
    {
        public delegate void OnServerUpdate();
        public delegate void OnUpdated();
    }
    
    public class Enums
    {
        public enum CurrentCharacter
        {
            Mimi,
            Vega
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