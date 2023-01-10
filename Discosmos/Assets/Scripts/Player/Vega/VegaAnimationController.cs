using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;

public class VegaAnimationController : AnimationController
{
   [Header("VEGA MODEL")]
   public SkinnedMeshRenderer weaponPart;

   public Material pinkWeaponMaterial;
   public Material greenWeaponMaterial;
   public Material neutralWeaponMaterial;

   public override void SetTeamMaterial()
   {
      base.SetTeamMaterial();
      
      switch (manager.currentTeam)
      {
         case Enums.Team.Green:
            weaponPart.material = greenWeaponMaterial;
            break;
         
         case Enums.Team.Pink:
            weaponPart.material = pinkWeaponMaterial;
            break;
         
         case Enums.Team.Neutral:
            weaponPart.material = neutralWeaponMaterial;
            break;
      }
   }

   public void CastVegaBlackHole()
   {
   }

   public void CastVegaUltimate()
   {
      
   }
}
