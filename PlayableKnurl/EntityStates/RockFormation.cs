using System;
using RoR2.Navigation;

namespace PlayableKnurl.EntityStates {
    public class RockFormation : BaseState {
        public override void OnEnter()
        {
            base.OnEnter();
            EffectManager.SpawnEffect(Utils.Paths.GameObject.TitanSpawnEffect.Load<GameObject>(), new EffectData {
                origin = base.transform.position,
                scale = 1f,
            }, true);
            AkSoundEngine.PostEvent(Events.Play_titanboss_m2_fist, base.gameObject);
            if (NetworkServer.active) {
                foreach (TeamComponent com in TeamComponent.GetTeamMembers(base.GetTeam())) {
                    if (com.body && com.body.bodyIndex == BodyCatalog.FindBodyIndex(Utils.Paths.GameObject.GolemBody28.Load<GameObject>())) {
                        com.body.healthComponent.Suicide();
                    }
                }
            }
            for (int i = 0; i < 3; i++) {
                NodeGraph graph = SceneInfo.instance.groundNodes;
                List<NodeGraph.Node> nodes = graph.nodes.Where(x => Vector3.Distance(x.position, base.transform.position) < 45).ToList();
                if (nodes.Count > 1 && NetworkServer.active) {
                    Vector3 pos = nodes.GetRandom().position;  
                    MasterSummon summon = new();
                    summon.ignoreTeamMemberLimit = false;
                    summon.masterPrefab = Utils.Paths.GameObject.GolemMaster.Load<GameObject>();
                    summon.rotation = Quaternion.identity;
                    summon.position = pos + (Vector3.up * 3);
                    summon.summonerBodyObject = base.gameObject;
                    summon.teamIndexOverride = base.GetTeam();
                    EffectManager.SpawnEffect(Utils.Paths.GameObject.TitanSpawnEffect.Load<GameObject>(), new EffectData {
                        origin = pos + (Vector3.up * 3),
                        scale = 1f,
                    }, true);
                    CharacterMaster master = summon.Perform();
                    if (master) {
                        master.inventory.GiveItem(RoR2Content.Items.Knurl, base.characterBody.inventory.GetItemCount(RoR2Content.Items.Knurl));
                    }
                }
            }

            if (NetworkServer.active) {
                base.characterBody.inventory.RemoveItem(RoR2Content.Items.Knurl, base.characterBody.inventory.GetItemCount(RoR2Content.Items.Knurl));
            }

            outer.SetNextStateToMain();
        }
    }
}