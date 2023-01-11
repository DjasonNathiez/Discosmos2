using UnityEngine;

public class MobsUIManager : MonoBehaviour
{
    public static MobsUIManager instance;
    public Transform canvas;

    private void Awake()
    {
        instance = this;
    }
}
