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
   public CapacityHitBox outsideBlackHole;
   public float attractForce = 1;
   public CapacityHitBox insideBlackHole;
   public float tickDamageRate;
   private float tickDamageTimer;
   private float timer;
   
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
            manager.SetCapacity1OnCooldown();
            blackHoleIsActive = false;
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
         OutsideBlackHoleEffect();
         InsideBlackHoleEffect();
      }
   }

   private void OutsideBlackHoleEffect()
   {
      foreach (GameObject target in outsideBlackHole.targets)
      {
         Rigidbody rb = target.GetComponent<Rigidbody>();
         rb.velocity = (transform.position - rb.transform.position).normalized * attractForce;
      }
   }

   private void InsideBlackHoleEffect()
   {
      
      foreach (GameObject target in outsideBlackHole.targets)
      {
         Rigidbody rb = target.GetComponent<Rigidbody>();
         rb.velocity = (transform.position - rb.transform.position).normalized * (attractForce * Time.deltaTime);
      }
      
      if (tickDamageTimer >= tickDamageRate)
      {
         manager.DealDamage(insideBlackHole.idOnIt.ToArray(), manager.capacity1.amount);
         tickDamageTimer = 0;
      }
      else
      {
         tickDamageTimer += Time.deltaTime;
      }
   }
   
   public void CastVegaBlackHole()
   {
      blackHoleVFX.transform.gameObject.SetActive(true);
      blackHoleVFX.Play();
      timer = 0;
      blackHoleIsActive = true;
      manager.capacity1InCooldown = true;
      manager.controller.EnableMovement();
   }

   public void CallVegaBlackHoleVFX()
   {
      
   }
   
   public void CastVegaUltimate()
   {
      
   }
}
