namespace Tools
{

    public class NetworkDelegate
    {
        public delegate void OnServerUpdate();
        public delegate void OnUpdated();
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