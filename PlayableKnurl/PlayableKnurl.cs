using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;

namespace PlayableKnurl {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class PlayableKnurl : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulse";
        public const string PluginName = "PlayableKnurl";
        public const string PluginVersion = "1.0.0";

        public static AssetBundle bundle;
        public static BepInEx.Logging.ManualLogSource ModLogger;
        // vars
        public static SkillDef KnurlPrimary;
        public static SkillDef KnurlSecondary;
        public static SkillDef KnurlUtility;
        public static SkillDef KnurlSpecial;
        public static GameObject KnurlBody;
        public static SurvivorDef sdKnurl;
        public static GameObject KnurlProjectile;
        public static BuffDef RegenBuff;
        public static SkinDef SkinKnurl;
        // damage types
        public static DamageAPI.ModdedDamageType KnurlOnDeath = DamageAPI.ReserveDamageType();

        public void Awake() {
            // assetbundle loading 
            bundle = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("PlayableKnurl.dll", "assetbundurl"));

            // set logger
            ModLogger = Logger;
            
            LoadAssets();
            AddLanguage();
            ModifyAssets();
            Hooks();
            ContentAddition.AddBody(KnurlBody);
            ContentAddition.AddSurvivorDef(sdKnurl);
            ContentAddition.AddProjectile(KnurlProjectile);
            ContentAddition.AddBuffDef(RegenBuff);
        }

        private void Hooks() {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, report) => {
                orig(self, report);
                if (NetworkServer.active && report.damageInfo.HasModdedDamageType(KnurlOnDeath)) {
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Items.Knurl.itemIndex), report.damageInfo.position, Vector3.up * 20);
                }
                else if (NetworkServer.active && report.attackerBodyIndex == BodyCatalog.FindBodyIndex(Utils.Paths.GameObject.GolemBody28.Load<GameObject>())) {
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Items.Knurl.itemIndex), report.damageInfo.position, Vector3.up * 20);
                }
            };

            RecalculateStatsAPI.GetStatCoefficients += (body, args) => {
                if (NetworkServer.active && body.HasBuff(RegenBuff)) {
                    float multiplier = 0.45f;
                    args.regenMultAdd += multiplier;
                    int count = body.inventory.GetItemCount(RoR2Content.Items.Knurl);
                    List<CharacterBody.TimedBuff> debuffs = body.timedBuffs.Where(x => BuffCatalog.GetBuffDef(x.buffIndex).isDebuff).ToList();
                    for (int i = 0; i < count; i++) {
                        if (debuffs.Count > 1) {
                            CharacterBody.TimedBuff buff = debuffs.GetRandom();
                            debuffs.Remove(buff);
                            body.RemoveBuff(buff.buffIndex);
                        }
                    }
                    if (count > 0) {
                        EffectData data = new();
                        data.origin = base.transform.position;
                        data.SetHurtBoxReference(body.mainHurtBox);
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/CleanseEffect"), data, transmit: true);
                    }
                }
            };

            On.RoR2.Skills.SkillDef.IsReady += (orig, self, slot) => {
                if (slot.skillDef == KnurlPrimary) {
                    return orig(self, slot) && slot.GetComponent<CharacterBody>().inventory.GetItemCount(RoR2Content.Items.Knurl) > 0;
                }
                else if (slot.skillDef == KnurlUtility) {
                    return orig(self, slot) && slot.GetComponent<CharacterBody>().inventory.GetItemCount(RoR2Content.Items.Knurl) > 1;
                }
                else if (slot.skillDef == KnurlSpecial) {
                    return orig(self, slot) && slot.GetComponent<CharacterBody>().inventory.GetItemCount(RoR2Content.Items.Knurl) > 0;
                }
                else {
                    return orig(self, slot);
                }
            };
        }

        private void ModifyAssets() {
            KnurlPrimary.activationState = ContentAddition.AddEntityState<EntityStates.KnurlHurl>(out bool _);
            DamageAPI.ModdedDamageTypeHolderComponent knurlTypeHolder = KnurlProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            knurlTypeHolder.Add(KnurlOnDeath);

            ProjectileSingleTargetImpact impact = KnurlProjectile.GetComponent<ProjectileSingleTargetImpact>();
            impact.impactEffect = Utils.Paths.GameObject.ImpactToolbotDashLarge.Load<GameObject>();

            KnurlSecondary.activationState = ContentAddition.AddEntityState<EntityStates.FocusRegen>(out bool _);
            KnurlUtility.activationState = ContentAddition.AddEntityState<EntityStates.StoneCharge>(out bool _);
            KnurlSpecial.activationState = ContentAddition.AddEntityState<EntityStates.RockFormation>(out bool _);

            SkinKnurl.icon = LoadoutAPI.CreateSkinIcon(Color.grey, Color.red, Color.black, Color.gray);
        }

        private void LoadAssets() {
            KnurlPrimary = bundle.LoadAsset<SkillDef>("Assets/PlayableKnurl/Skills/SkillDefs/Primary.asset");
            KnurlSecondary = bundle.LoadAsset<SkillDef>("Assets/PlayableKnurl/Skills/SkillDefs/Secondary.asset");
            KnurlUtility = bundle.LoadAsset<SkillDef>("Assets/PlayableKnurl/Skills/SkillDefs/Utility.asset");
            KnurlSpecial = bundle.LoadAsset<SkillDef>("Assets/PlayableKnurl/Skills/SkillDefs/Special.asset");
            KnurlBody = bundle.LoadAsset<GameObject>("Assets/PlayableKnurl/PlayableKnurlBody.prefab");
            sdKnurl = bundle.LoadAsset<SurvivorDef>("Assets/PlayableKnurl/PlayableKnurl.asset");
            KnurlProjectile = bundle.LoadAsset<GameObject>("Assets/PlayableKnurl/Skills/Projectiles/KnurlProjectile.prefab");
            RegenBuff = bundle.LoadAsset<BuffDef>("Assets/PlayableKnurl/Skills/Buffs/KnurlRegen.asset");
            SkinKnurl = bundle.LoadAsset<SkinDef>("Assets/PlayableKnurl/SkinKnurl.asset");
        }

        private void AddLanguage() {
            "KNURL_PRIMARY_NAME".Add("Knurl Hurl");
            "KNURL_PRIMARY_DESC".Add("Throw <style=cIsUtility>Titanic Knurls</style> for <style=cIsDamage>320% damage</style>. Kills drop a <style=cIsHealing>Titanic Knurl</style>. Costs 1 <style=cIsHealing>Titanic Knurl</style>.");
            "KNURL_SECONDARY_NAME".Add("Titanic Rejuvenation");
            "KNURL_SECONDARY_DESC".Add("Focus, gaining <style=cIsUtility>+45% increased regen</style> for a short time. Gain 6 <style=cIsHealing>Titanic Knurls</style>.");
            "KNURL_UTILITY_NAME".Add("Stone Charge");
            "KNURL_UTILITY_DESC".Add("<style=cIsUtility>Agile</style>. <style=cIsUtility>Charge forward</style>, gaining <style=cIsDamage>100 armor</style> temporarily. Costs 2 <style=cIsHealing>Titanic Knurls</style>");
            "KNURL_SPECIAL_NAME".Add("Rock Formation");
            "KNURL_SPECIAL_DESC".Add("Summon an army of <style=cIsDamage>Stone Golems</style> to defend you. Requires <style=cIsDamage>Titanic Knurls</style>. Golem kills drop <style=cIsHealing>Titanic Knurls</style>.");
            "KNURL_PASSIVE_NAME".Add("Colossal Spirit");
            "KNURL_PASSIVE_DESC".Add("<style=cIsDamage>Titanic Knurls</style> <style=cIsUtility>empower</style> your abilities.");
            "KNURL_SURVIVOR_NAME".Add("Titanic Knurl");
            "KNURL_SKIN_NAME".Add("Standard");
            "KNURL_SURVIVOR_DESC".Add(
                """
                Subject: Titanic Knurl
                Technician: C. Foreman
                Table Spec: Mineral Analysis BFC-5
                Notes: 

                > From initial inspection, Knurl seems to be comprised of non-metallic substances. No marks are left when Knurl is r믭 against test surface.
                > We inspect hardness of Knurl. We managed to chip some of the Knurl away, showing us that the Knurl was tough but granular – individual fragments could be removed with little effort.
                > The fragment is moving. It appears to be trying to rejoin the mass.
                > Out of curiosity, I let it move freely. It slides along the table, up the knurl, and deposits itself back to its original position. The seam lines fade and the knurl is back as it had been minutes ago.
                > I test for magnetic properties in the rock. None are found.
                > Knurl’s regenerative properties are documented, but are unexplainable at this moment.
                > Knurl is slowly moving off the table as I write this.
                """
            );
            "KNURL_UPGRADE_PRIMARY".Add("<style=cKeywordName>Titanic Boost</style>Fire 7.5% faster for every Titanic Knurl you have.");
            "KNURL_UPGRADE_SECONDARY".Add("<style=cKeywordName>Titanic Boost</style>Cleanse 1 debuff for every Titanic Knurl you have.");
            "KNURL_UPGRADE_UTILITY".Add("<style=cKeywordName>Titanic Boost</style>Charge 5% faster for every Ttianic Knurl you have.");
            "KNURL_UPGRADE_SPECIAL".Add("<style=cKeywordName>Titanic Boost</style>The Stone Golems inherit every Titanic Knurl you have.");
            "KNURL_SURVIVOR_OUTRO".Add("And so it left, with it's high consistency...");
            "KNURL_SURVIVOR_FAILURE".Add("And so it vanished, never to be scrapped for shatterspleen again...");
            "KNURL_BODY_NAME".Add("Titanic Knurl");
            "KNURL_BODY_SUBTITLE".Add("Bulwark of Consistency");
        }
    }
}