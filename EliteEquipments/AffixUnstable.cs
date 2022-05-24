﻿using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Thalassophobia.EliteEquipments
{
    class AffixUnstable : EliteEquipmentBase<AffixUnstable>
    {
        public override string EliteEquipmentName => "Blessing Of The Abyss";

        public override string EliteAffixToken => "AFFIX_UNSTABLE";

        public override string EliteEquipmentPickupDesc => "Become an aspect of the Void.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Erythrite";

        public override GameObject EliteEquipmentModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Sprite EliteEquipmentIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public override Sprite EliteBuffIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public override Color32 EliteColor => new Color32(125, 75, 75, byte.MaxValue);

        public override int EliteRampIndex => 3;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
        }

        private void CreateEliteTiers()
        {
            CanAppearInEliteTiers = new CombatDirector.EliteTierDef[]
            {
                //R2API.EliteAPI.VanillaFirstTierDef
            };
        }

        private bool SetAvailability(SpawnCard.EliteRules arg)
        {
            return Run.instance.loopClearCount > 0 && arg == SpawnCard.EliteRules.Default;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {

        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }
}
