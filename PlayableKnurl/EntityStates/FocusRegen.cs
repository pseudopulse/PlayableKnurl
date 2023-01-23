using System;

namespace PlayableKnurl.EntityStates {
    public class FocusRegen : BaseState {
        private float duration = 2f;
        public override void OnEnter()
        {
            base.OnEnter();
            AkSoundEngine.PostEvent(Events.Play_voidman_R_activate, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }

            characterBody.SetSpreadBloom(Util.Remap(base.fixedAge, 0f, duration, 0, 1));
        }

        public override void OnExit()
        {
            base.OnExit();
            if (NetworkServer.active) {
                base.characterBody.AddTimedBuff(PlayableKnurl.RegenBuff, 6f, 1);
                base.characterBody.inventory.GiveItem(RoR2Content.Items.Knurl, 6);
            }
            EffectManager.SpawnEffect(Utils.Paths.GameObject.FractureImpactEffect.Load<GameObject>(), new EffectData {
                origin = base.transform.position,
                scale = 2.5f
            }, true);
            AkSoundEngine.PostEvent(Events.Play_voidman_R_pop, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}