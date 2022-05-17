﻿using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Thalassophobia.Items.Tier3
{
    public class ProcOnKills : ItemBase<ProcOnKills>
    {
        public override string ItemName => "Volatile Concoction";

        public override string ItemLangTokenName => "PROC_ON_KILLS";

        public override string ItemPickupDesc => "Trigger on kill effects on hit.";

        public override string ItemFullDescription => "";

        public override string ItemLore => "Order: Armor-Piercing Rounds, 50mm\nTracking Number: 15***********\nEstimated Delivery: 3/07/2056\n" +
            "Shipping Method: Standard\nShipping Address: Fort Margaret, Jonesworth System\n" +
            "Shipping Details:\n" +
            "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        // Item stats
        private float damageThreshold;
        private float cooldown;
        private float damageBonus;

        static BuffDef procCooldown;
        static BuffDef procReady;

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

            damageThreshold = config.Bind<float>("Item: " + ItemName, "DamageThreshold", 450f, "Amount of damage needed to trigger the effect.").Value;
            cooldown = config.Bind<float>("Item: " + ItemName, "Cooldown", 2f, "Cooldown between triggering the effect.").Value;
            damageBonus = config.Bind<float>("Item: " + ItemName, "Damage", 0.25f, "Damage bonus to on kill effects.").Value;

            procCooldown = ScriptableObject.CreateInstance<BuffDef>();
            procCooldown.name = "Concoction Cooldown";
            procCooldown.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            procCooldown.canStack = false;
            procCooldown.isDebuff = false;
            procCooldown.isCooldown = true;
            procCooldown.buffColor = Color.yellow;
            ContentAddition.AddBuffDef(procCooldown);

            procReady = ScriptableObject.CreateInstance<BuffDef>();
            procReady.name = "Concoction Ready";
            procReady.iconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            procReady.canStack = false;
            procReady.isDebuff = false;
            procReady.isCooldown = false;
            procReady.buffColor = Color.red;
            ContentAddition.AddBuffDef(procReady);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnInventoryChanged += (orig, self) =>
            {
                orig(self);
                if (self && self.GetBody())
                {
                    self.GetBody().AddItemBehavior<OnKillProc>(GetCount(self.GetBody()));
                }
            };


            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);
                if (damageInfo.attacker)
                {
                    CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
                    var potionCount = GetCount(body);
                    if (body.HasBuff(procReady))
                    {
                        if (damageInfo.damage >= damageInfo.attacker.GetComponent<CharacterBody>().damage * 4 && victim.GetComponent<CharacterBody>())
                        {
                            if (Plugin.DEBUG) 
                            {
                                Log.LogInfo("Triggered Concoction");
                            }
                            DamageInfo damageInfo2 = new DamageInfo
                            {
                                attacker = damageInfo.attacker,
                                crit = damageInfo.crit,
                                damage = damageInfo.damage,
                                position = victim.GetComponent<CharacterBody>().transform.position,
                                procCoefficient = damageInfo.procCoefficient,
                                damageType = damageInfo.damageType,
                                damageColorIndex = damageInfo.damageColorIndex
                            };
                            HealthComponent victim2 = victim.GetComponent<CharacterBody>().healthComponent;
                            DamageReport damageReport = new DamageReport(damageInfo, victim2, damageInfo.damage, victim2.combinedHealth);
                            GlobalEventManager.instance.OnCharacterDeath(damageReport);
                            body.RemoveBuff(procReady);
                            body.AddTimedBuff(procCooldown, 2.0f);
                        }
                    }
                }
            };
        }

        public class OnKillProc : CharacterBody.ItemBehavior
        {
            private void Start()
            {
                if (!(this.body.HasBuff(procCooldown) || this.body.HasBuff(procCooldown)))
                {
                    this.body.AddTimedBuff(procCooldown, 2.0f);
                }
            }

            private void OnDisable()
            {
                if (this.body)
                {
                    if (this.body.HasBuff(procReady))
                    {
                        this.body.RemoveBuff(procReady);
                    }
                    if (this.body.HasBuff(procCooldown))
                    {
                        this.body.RemoveBuff(procCooldown);
                    }
                }
            }
            private void FixedUpdate()
            {
                bool flag = this.body.HasBuff(procCooldown);
                bool flag2 = this.body.HasBuff(procReady);
                if (!flag && !flag2)
                {
                    if (Plugin.DEBUG)
                    {
                        Log.LogInfo("Concoction Ready");
                    }
                    this.body.AddBuff(procReady);
                }
                if (flag2 && flag)
                {
                    this.body.RemoveBuff(procReady);
                }
            }
        }
    }
}