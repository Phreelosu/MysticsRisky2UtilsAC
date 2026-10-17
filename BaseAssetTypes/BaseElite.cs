using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MysticsRisky2Utils.ContentManagement;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace MysticsRisky2Utils.BaseAssetTypes
{
    public abstract class BaseElite : BaseLoadableAsset
    {
        public EliteDef eliteDef;
        public int vanillaTier = 0;
        public bool isHonor = false;
        public GameObject modelEffect;
        public delegate void OnModelEffectSpawn(CharacterModel model, GameObject effect);
        public OnModelEffectSpawn onModelEffectSpawn;
        public Color? lightColorOverride;
        public Material particleMaterialOverride;

        public static List<BaseElite> elites = new List<BaseElite>();
        public static Dictionary<EliteIndex, BaseElite> elitesByIndex = new Dictionary<EliteIndex, BaseElite>();

        public override void Load()
        {
            eliteDef = ScriptableObject.CreateInstance<EliteDef>();
            OnLoad();
            eliteDef.modifierToken = "ELITE_MODIFIER_" + eliteDef.name.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            asset = eliteDef;
            elites.Add(this);
        }

        internal static void Init()
        {
            On.RoR2.EliteCatalog.Init += EliteCatalog_Init;
            On.RoR2.EliteDef.IsAvailable += (orig, self) =>
            {
                if (self.eliteEquipmentDef == null) return false;
                return orig(self);
            };
            On.RoR2.CharacterModel.UpdateLights += CharacterModel_UpdateLights;
            On.RoR2.CharacterModel.OnDestroy += CharacterModel_OnDestroy;

            On.RoR2.CombatDirector.Init += CombatDirector_Init;
        }

        private static void EliteCatalog_Init(On.RoR2.EliteCatalog.orig_Init orig)
        {
            orig();
            foreach (var elite in elites)
            {
                elitesByIndex[elite.eliteDef.eliteIndex] = elite;
            }
            // Artifact of Honor picks scripted encounter elites (Alloy Worship Unit, Aurelionite...) from this list;
            // IsAvailable() is checked on each spawn
            foreach (var elite in elites.FindAll(x => x.isHonor))
            {
                HG.ArrayUtils.ArrayAppend(ref RoR2.Artifacts.EliteOnlyArtifactManager.eliteDefs, elite.eliteDef);
            }
        }

        private class ActiveModelEffectInfo
        {
            public GameObject instance;
            public BaseElite elite;
        }
        private static Dictionary<CharacterModel, ActiveModelEffectInfo> modelEffects = new Dictionary<CharacterModel, ActiveModelEffectInfo>();

        private static void CharacterModel_UpdateLights(On.RoR2.CharacterModel.orig_UpdateLights orig, CharacterModel self)
        {
            BaseElite currentCustomElite = null;
            ActiveModelEffectInfo modelEffect = null;

            if (elitesByIndex.TryGetValue(self.myEliteIndex, out currentCustomElite))
            {
                self.lightColorOverride = currentCustomElite.lightColorOverride;
                self.particleMaterialOverride = currentCustomElite.particleMaterialOverride;
            }

            if (modelEffects.TryGetValue(self, out modelEffect))
            {
                if (currentCustomElite != modelEffect.elite)
                {
                    Object.Destroy(modelEffect.instance);
                    modelEffects.Remove(self);
                    modelEffect = null;
                }
            }

            if (currentCustomElite != null && modelEffect == null && currentCustomElite.modelEffect != null)
            {
                var modelEffectInstance = Object.Instantiate(currentCustomElite.modelEffect, self.transform);
                if (currentCustomElite.onModelEffectSpawn != null)
                    currentCustomElite.onModelEffectSpawn(self, modelEffectInstance);

                modelEffects[self] = new ActiveModelEffectInfo
                {
                    elite = currentCustomElite,
                    instance = modelEffectInstance
                };
            }

            orig(self);
        }

        private static void CharacterModel_OnDestroy(On.RoR2.CharacterModel.orig_OnDestroy orig, CharacterModel self)
        {
            orig(self);
            if (modelEffects.ContainsKey(self))
                modelEffects.Remove(self);
        }

        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            foreach (BaseElite customElite in elites)
            {
                switch (customElite.vanillaTier)
                {
                    case 1:
                        HG.ArrayUtils.ArrayAppend(ref R2API.EliteAPI.VanillaEliteTiers[(int)R2API.VanillaEliteTier.BaseTier1].eliteTypes, customElite.eliteDef);
                        HG.ArrayUtils.ArrayAppend(ref R2API.EliteAPI.VanillaEliteTiers[(int)R2API.VanillaEliteTier.FullTier1].eliteTypes, customElite.eliteDef);
                        break;
                    case 2:
                        HG.ArrayUtils.ArrayAppend(ref R2API.EliteAPI.VanillaEliteTiers[(int)R2API.VanillaEliteTier.Tier2].eliteTypes, customElite.eliteDef);
                        break;
                }
                if (customElite.isHonor && customElite.vanillaTier == 1)
                {
                    HG.ArrayUtils.ArrayAppend(ref R2API.EliteAPI.VanillaEliteTiers[(int)R2API.VanillaEliteTier.BaseTier1Honor].eliteTypes, customElite.eliteDef);
                    HG.ArrayUtils.ArrayAppend(ref R2API.EliteAPI.VanillaEliteTiers[(int)R2API.VanillaEliteTier.FullTier1Honor].eliteTypes, customElite.eliteDef);
                }
            }
        }
    }
}
