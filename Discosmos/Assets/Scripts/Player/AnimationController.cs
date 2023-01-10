using Tools;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [Header("REFERENCES")] 
    public PlayerManager manager;
    
    [Header("MODEL")] 
    public SkinnedMeshRenderer[] bodyParts;
    public Material pinkMaterial;
    public Material greenMaterial;
    public Material neutralMaterial;
    
    [Header("CAPACITIES")]
    public int capacity1Index;
    public int capacity2Index;

    public virtual void SetTeamMaterial()
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            switch (manager.currentTeam)
            {
                case Enums.Team.Green:
                    if (greenMaterial) bodyParts[i].material = greenMaterial;
                    break;
                
                case Enums.Team.Pink:
                    if (pinkMaterial) bodyParts[i].material = pinkMaterial;
                    break;
                
                case Enums.Team.Neutral:
                    if (neutralMaterial) bodyParts[i].material = neutralMaterial;
                    break;
            }
        }
    }
}
