using System;

namespace PlayableKnurl.EntityStates {
    public class KnurlHurl : BaseState {
        private float delay = 1.1f;
        private float damageCoefficient = 3.2f;
        private GameObject prefab => PlayableKnurl.KnurlProjectile;

        public override void OnEnter()
        {
            base.OnEnter();
            delay = delay / attackSpeedStat;
            delay = delay / (1f + (0.075f * base.characterBody.inventory.GetItemCount(RoR2Content.Items.Knurl)));

            if (base.isAuthority) {
                FireProjectileInfo info = new();
                info.damage = base.damageStat * damageCoefficient;
                info.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(base.GetAimRay().direction, -1f, 1f, 1f, 1f));
                info.position = base.GetAimRay().origin;
                info.projectilePrefab = prefab;
                info.crit = base.RollCrit();
                info.owner = base.gameObject;

                ProjectileManager.instance.FireProjectile(info);
            }

            AkSoundEngine.PostEvent(Events.Play_MULT_m1_grenade_launcher_shoot, base.gameObject);
            base.characterBody.inventory.RemoveItem(RoR2Content.Items.Knurl);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= delay) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}