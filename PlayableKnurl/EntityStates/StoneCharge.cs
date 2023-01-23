using System;

namespace PlayableKnurl.EntityStates {
    public class StoneCharge : BaseState {
        private float buffDuration = 2f;
        private float speed = 5000f;

        public override void OnEnter()
        {
            base.OnEnter();
            speed = speed * (1f + (0.05f * base.characterBody.inventory.GetItemCount(RoR2Content.Items.Knurl)));
            if (base.characterMotor) {
                base.characterMotor.ApplyForce(base.inputBank.moveVector * speed, true, false);
            }
            AkSoundEngine.PostEvent(Events.Play_item_use_fireballDash_explode, base.gameObject);
            if (NetworkServer.active) {
                base.characterBody.AddTimedBuff(RoR2Content.Buffs.SmallArmorBoost, buffDuration);
                base.characterBody.inventory.RemoveItem(RoR2Content.Items.Knurl, 2);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= 0.5f) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}