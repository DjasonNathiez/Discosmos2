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

   [Header("BLACK HOLE")] 
   public bool blackHoleIsActive;
   public ParticleSystem blackHoleVFX;
   public CapacityHitBox insideBlackHole;
   private float timer;
   public List<int> blackHoleHitted;
   public List<float> hittedTimers;

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

   public override void UpdateInDependencies()
   {
      base.UpdateInDependencies();

      if (blackHoleIsActive)
      {
         if (timer >= manager.capacity1.durationBase)
         {
            blackHoleVFX.Stop();
            blackHoleVFX.transform.gameObject.SetActive(false);
            //manager.SetCapacity1OnCooldown();
            blackHoleIsActive = false;
            manager.controller.ChangeAnimation(1);
            manager.controller.movementType = Enums.MovementType.KeepDirection;
            manager.force *= 0.2f;
         }
         else
         {
            timer += Time.deltaTime;
         }
      }
   }

   private void FixedUpdate()
   {
      if (blackHoleIsActive)
      {
         InsideBlackHoleEffect();
      }
   }

   private void InsideBlackHoleEffect()
   {
      for (int i = 0; i < insideBlackHole.idOnIt.Count; i++)
      {
         if (!blackHoleHitted.Contains(insideBlackHole.idOnIt[i]))
         {
            blackHoleHitted.Add(insideBlackHole.idOnIt[i]);
            int[] idArray = new[] {1};
            idArray[0] = insideBlackHole.idOnIt[i];
            manager.DealDamage(idArray, manager.capacity1.amount); 
            manager.HitStop(idArray,  0.3f, 0.3f);
            manager.KnockBack(idArray,0.45f + 0.3f * manager.force,9f + 3f * manager.force,insideBlackHole.targets[i].transform.position - transform.position);
            hittedTimers.Add(1);
         }
         else
         {
            if (hittedTimers[i] <= 0)
            {
               hittedTimers.RemoveAt(i);
               blackHoleHitted.RemoveAt(i);
               break;
            }
         }
      }

      for (int i = 0; i < hittedTimers.Count; i++)
      {
         hittedTimers[i] -= Time.deltaTime;
      }
   }
   
   public void CastVegaBlackHole()
   {
      if (!blackHoleIsActive)
      {
         blackHoleVFX.transform.gameObject.SetActive(true);
         blackHoleVFX.Play();
         timer = 0;
         blackHoleIsActive = true;
         manager.capacity1InCooldown = true;
         manager.controller.EnableMovement();
         manager.controller.movementType = Enums.MovementType.Tornado;
         Vector3 mouse = manager.controller.MouseWorldPosition();
         manager.controller.direction = (new Vector3(mouse.x, transform.position.y, mouse.z) - transform.position).normalized;
      }
   }

   public void CallVegaBlackHoleVFX()
   {
      
   }
   
   public void CastVegaUltimate()
   {
      
   }
}