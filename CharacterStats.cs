using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MysticsRisky2Utils.MysticsRisky2UtilsPlugin;

namespace MysticsRisky2Utils
{
    [Obsolete("for EliteVariety support, dont use this (use R2API instead)")]
    public class CharacterStats
    {
        public delegate float StatModifierApplyTimes(GenericCharacterInfo genericCharacterInfo);
        public delegate bool StatModifierShouldApply(GenericCharacterInfo genericCharacterInfo);
        public struct StatModifier
        {
            public float multiplier;

            public float flat;

            public StatModifierApplyTimes times;
        }
        public struct FlatStatModifier
        {
            public float amount;

            public StatModifierApplyTimes times;
        }
        public struct BoolStatModifier
        {
            public StatModifierShouldApply shouldApply;
        }

        public static List<FlatStatModifier> levelModifiers = new List<FlatStatModifier>();
        public static List<StatModifier> healthModifiers = new List<StatModifier>();
        public static List<StatModifier> shieldModifiers = new List<StatModifier>();
        public static List<FlatStatModifier> regenModifiers = new List<FlatStatModifier>();
        public static List<StatModifier> moveSpeedModifiers = new List<StatModifier>();
        public static List<BoolStatModifier> rootMovementModifiers = new List<BoolStatModifier>();
        public static List<StatModifier> damageModifiers = new List<StatModifier>();
        public static List<StatModifier> attackSpeedModifiers = new List<StatModifier>();
        public static List<FlatStatModifier> critModifiers = new List<FlatStatModifier>();
        public static List<FlatStatModifier> armorModifiers = new List<FlatStatModifier>();
        public static List<StatModifier> cooldownModifiers = new List<StatModifier>();
        public static List<FlatStatModifier> cursePenaltyModifiers = new List<FlatStatModifier>();

        public static void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                GenericCharacterInfo info = new GenericCharacterInfo(self);
                AddStatModifier(info, levelModifiers, ref args.levelFlatAdd);
                AddStatModifier(info, healthModifiers, ref args.baseHealthAdd, ref args.healthMultAdd);
                AddStatModifier(info, shieldModifiers, ref args.baseShieldAdd, ref args.shieldMultAdd);
                AddStatModifier(info, regenModifiers, ref args.baseRegenAdd);
                AddStatModifier(info, cursePenaltyModifiers, ref args.baseCurseAdd);
                AddStatModifier(info, damageModifiers, ref args.baseDamageAdd, ref args.damageMultAdd);
                AddStatModifier(info, attackSpeedModifiers, ref args.baseAttackSpeedAdd, ref args.attackSpeedMultAdd);
                AddStatModifier(info, moveSpeedModifiers, ref args.baseMoveSpeedAdd, ref args.moveSpeedMultAdd);
                AddStatModifier(info, cooldownModifiers, ref args.cooldownReductionAdd, ref args.cooldownMultAdd);
                AddStatModifier(info, critModifiers, ref args.critAdd);
                AddStatModifier(info, armorModifiers, ref args.armorAdd);
                foreach (var m in rootMovementModifiers) if (m.shouldApply(info))
                {
                    self.moveSpeed = 0f;
                    self.acceleration = 80f;
                }
            };
        }

        public static void AddStatModifier(GenericCharacterInfo info, List<StatModifier> list, ref float flat, ref float mult)
        {
            foreach (var m in list) if (m.times(info) != 0)
            {
                if (m.flat != 0) flat += m.flat * m.times(info);
                if (m.multiplier != 0) mult += m.multiplier * m.times(info);
            }
        }

        public static void AddStatModifier(GenericCharacterInfo info, List<FlatStatModifier> list, ref float flat)
        {
            foreach (var m in list) if (m.amount != 0 && m.times(info) != 0) flat += m.amount * m.times(info);
        }
    }
}
