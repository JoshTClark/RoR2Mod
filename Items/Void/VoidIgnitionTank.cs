﻿using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using Thalassophobia.Items.Lunar;
using Thalassophobia.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;
using static RoR2.DotController;

namespace Thalassophobia.Items.Void
{
    class VoidIgnitionTank : ItemBase<VoidIgnitionTank>
    {
        public override string ItemName => "Infected Aerosol";

        public override string ItemLangTokenName => "VOID_IGNITION_TANK";

        public override string ItemPickupDesc => "Corrupts all Ignition Tanks.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override String CorruptsItem => DLC1Content.Items.StrengthenBurn.nameToken;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Buff
        public BuffDef voidFog;

        // Damage Type
        ModdedDamageType inflictVoid;

        // Stats
        public float tickPeriodSeconds;
        public float healthFractionPerTick;
        public float healthFractionRampCoefficientPerTick;
        public float duration;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ItemTags = new ItemTag[] { ItemTag.Damage };

            tickPeriodSeconds = config.Bind<float>("Item: " + ItemName, "tickPeriodSeconds", 0.4f, "").Value;
            healthFractionPerTick = config.Bind<float>("Item: " + ItemName, "healthFractionPerTick", 0.01f, "").Value;
            healthFractionRampCoefficientPerTick = config.Bind<float>("Item: " + ItemName, "healthFractionRampCoefficientPerTick", 0.1f, "").Value;
            duration = config.Bind<float>("Item: " + ItemName, "duration", 2.5f, "").Value;

            inflictVoid = ReserveDamageType();

            voidFog = ScriptableObject.CreateInstance<BuffDef>();
            voidFog.name = "Suffocating";
            voidFog.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            voidFog.canStack = false;
            voidFog.isDebuff = true;
            ContentAddition.AddBuffDef(voidFog);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.ProcIgniteOnKill += GlobalEventManager_ProcIgniteOnKill;

            On.RoR2.StrengthenBurnUtils.CheckDotForUpgrade += StrengthenBurnUtils_CheckDotForUpgrade;
        }

        private void GlobalEventManager_ProcIgniteOnKill(On.RoR2.GlobalEventManager.orig_ProcIgniteOnKill orig, DamageReport damageReport, int igniteOnKillCount, CharacterBody victimBody, TeamIndex attackerTeamIndex)
        {
            SphereZone zone = new SphereZone();
            zone.radius = 10.0f;
            zone.transform.position = victimBody.transform.position;
            orig(damageReport, igniteOnKillCount, victimBody, attackerTeamIndex);
        }

        private void StrengthenBurnUtils_CheckDotForUpgrade(On.RoR2.StrengthenBurnUtils.orig_CheckDotForUpgrade orig, Inventory inventory, ref InflictDotInfo dotInfo)
        {
            orig(inventory, ref dotInfo);
            if (dotInfo.dotIndex == DotController.DotIndex.Burn || dotInfo.dotIndex == DotController.DotIndex.Helfire || dotInfo.dotIndex == DotController.DotIndex.StrongerBurn)
            {
                int itemCount = GetCount(dotInfo.attackerObject.GetComponent<CharacterBody>());
                if (itemCount > 0)
                {
                    dotInfo.dotIndex = DotIndex.None;
                    CharacterBody victim = dotInfo.victimObject.GetComponent<CharacterBody>();

                    victim.AddTimedBuff(voidFog, duration);
                    if (!victim.gameObject.GetComponent<FogController>())
                    {
                       FogController controller = victim.gameObject.AddComponent<FogController>();
                        controller.victim = victim;
                        controller.attacker = dotInfo.attackerObject.GetComponent<CharacterBody>();
                        controller.buff = voidFog;
                        controller.tickPeriodSeconds = tickPeriodSeconds;
                        controller.healthFractionPerSecond = healthFractionPerTick * (1.0f/tickPeriodSeconds);
                        controller.healthFractionRampCoefficientPerSecond = healthFractionRampCoefficientPerTick * (1.0f / tickPeriodSeconds);
                    }
                }
            }
        }
    }
}