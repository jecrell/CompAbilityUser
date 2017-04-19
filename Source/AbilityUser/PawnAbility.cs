using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace AbilityUser
{
    //public class PawnAbility : ThingWithComps
    public class PawnAbility : IExposable
    {
        public Pawn pawn;
        private List<ThingComp> comps = new List<ThingComp>();
        public AbilityDef powerdef;
        public CompProperties MainEffectProps;
        public Texture2D PowerButton;
        public int TicksUntilCasting = -1;

        public PawnAbility()
        {

        }

        public PawnAbility(CompAbilityUser comp)
        {
            this.pawn = comp.abilityUser;
        }

        public PawnAbility(Pawn user, AbilityDef pdef)
        {
            this.pawn = user;
            this.powerdef = pdef;
            //this.def = pdef;
            this.PowerButton = pdef.uiIcon;
            //this.InitializePawnComps(user);
            //ThingIDMaker.GiveIDTo(this);
        }

        public void PawnAbilityTick()
        {
            if (TicksUntilCasting > -1) TicksUntilCasting--;
        }

        public virtual string GetDescription()
        {
            string result = "";
            string coolDesc = GetBasics();
            string AoEDesc = GetAoEDesc();
            string postDesc = PostAbilityVerbDesc();
            StringBuilder desc = new StringBuilder();
            desc.AppendLine(powerdef.description);
            if (coolDesc != "") desc.AppendLine(coolDesc);
            if (AoEDesc != "") desc.AppendLine(AoEDesc);
            if (postDesc != "") desc.AppendLine(postDesc);
            result = desc.ToString();
            return result;
        }

        public virtual string GetAoEDesc()
        {
            string result = "";
            VerbProperties_Ability def = powerdef.MainVerb;
            if (def != null)
            {
                if (def.TargetAoEProperties != null)
                {
                    StringBuilder s = new StringBuilder();
                    s.AppendLine("AU_AoEProperties".Translate());
                    if (def.TargetAoEProperties.targetClass == typeof(Pawn))
                        s.AppendLine("\t" + "AU_TargetClass".Translate() + "AU_AoECharacters".Translate());
                    else
                        s.AppendLine("\t" + "AU_TargetClass".Translate() + def.TargetAoEProperties.targetClass.ToString().CapitalizeFirst());
                    s.AppendLine("\t" + "Range".Translate() + ": " + def.TargetAoEProperties.range.ToString());
                    s.AppendLine("\t" + "AU_AoEFriendlyFire".Translate() + def.TargetAoEProperties.friendlyFire.ToString());
                    s.AppendLine("\t" + "AU_AoEMaxTargets".Translate() + def.TargetAoEProperties.maxTargets.ToString());
                    if (def.TargetAoEProperties.startsFromCaster)
                    {
                        s.AppendLine("\t" + "AU_AoEStartsFromCaster".Translate());
                    }
                    result = s.ToString();
                }
            }
            return result;
        }

        public virtual string PostAbilityVerbDesc()
        {
            return "";
        }

        public string GetBasics()
        {
            string result = "";
            VerbProperties_Ability def = powerdef.MainVerb;
            if (def != null)
            {
                StringBuilder s = new StringBuilder();
                s.AppendLine("AU_Cooldown".Translate() + def.SecondsToRecharge.ToString("N0") + " " + "SecondsLower".Translate());
                switch (def.AbilityTargetCategory)
                {
                    case AbilityTargetCategory.TargetAoE:
                        s.AppendLine("AU_Type".Translate() + "AU_TargetAoE".Translate());
                        break;
                    case AbilityTargetCategory.TargetSelf:
                        s.AppendLine("AU_Type".Translate() + "AU_TargetSelf".Translate());
                        break;
                    case AbilityTargetCategory.TargetThing:
                        s.AppendLine("AU_Type".Translate() + "AU_TargetThing".Translate());
                        break;
                }
                if (def.tooltipShowProjectileDamage)
                {
                    if (def.projectileDef != null)
                    {
                        if (def.projectileDef.projectile != null)
                        {
                            if (def.projectileDef.projectile.damageAmountBase > 0)
                            {
                                s.AppendLine("Damage".Translate() + ": " + def.projectileDef.projectile.damageAmountBase);
                                s.AppendLine("Damage".Translate() + " " + "AU_Type".Translate() + def.projectileDef.projectile.damageDef.LabelCap);
                            }
                        }
                    }
                }
                if (def.tooltipShowExtraDamages)
                {
                    if (def.extraDamages != null && def.extraDamages.Count > 0)
                    {
                        if (def.extraDamages.Count == 1)
                        {
                            s.AppendLine("AU_Extra".Translate() + " " + "Damage".Translate() + ": " + def.extraDamages[0].damage);
                            s.AppendLine("AU_Extra".Translate() + " " + "Damage".Translate() + " " + "AU_Type".Translate() + def.extraDamages[0].damageDef.LabelCap);
                        }
                        else
                        {
                            s.AppendLine("AU_Extra".Translate() + " " + "Damage".Translate() + ": ");
                            foreach (ExtraDamage extraDam in def.extraDamages)
                            {
                                s.AppendLine("\t" + "AU_Extra".Translate() + " " + "Damage".Translate() + " " + "AU_Type".Translate() + extraDam.damageDef.LabelCap);
                                s.AppendLine("\t" + "AU_Extra".Translate() + " " + "Damage".Translate() + ": " + extraDam.damage);
                            }
                        }
                    }
                }
                if (def.tooltipShowMentalStatesToApply)
                {
                    if (def.mentalStatesToApply != null && def.mentalStatesToApply.Count > 0)
                    {
                        if (def.mentalStatesToApply.Count == 1)
                        {
                            s.AppendLine("AU_MentalStateChance".Translate() + ": " + def.mentalStatesToApply[0].mentalStateDef.LabelCap + " " + def.mentalStatesToApply[0].applyChance.ToStringPercent());
                        }
                        else
                        {
                            s.AppendLine("AU_MentalStateChance".Translate());
                            foreach (ApplyMentalStates mentalState in def.mentalStatesToApply)
                            {
                                s.AppendLine("\t" + mentalState.mentalStateDef.LabelCap + " " + mentalState.applyChance.ToStringPercent());
                            }

                        }
                    }
                }
                if (def.tooltipShowHediffsToApply)
                {
                    if (def.hediffsToApply != null && def.hediffsToApply.Count > 0)
                    {
                        if (def.hediffsToApply.Count == 1)
                        {
                            s.AppendLine("AU_EffectChance".Translate() + def.hediffsToApply[0].hediffDef.LabelCap + " " + def.hediffsToApply[0].applyChance.ToStringPercent());
                        }
                        else
                        {
                            s.AppendLine("AU_EffectChance".Translate());
                            foreach (ApplyHediffs hediff in def.hediffsToApply)
                            {
                                s.AppendLine("\t" + hediff.hediffDef.LabelCap + " " + hediff.applyChance.ToStringPercent());
                            }

                        }
                    }
                }

                result = s.ToString();

            }
            return result;
        }

        public bool CanFire
        {
            get
            {
                if (TicksUntilCasting == -1 || TicksUntilCasting < 0) return true;
                return false;
            }
        }

        public int MaxCastingTicks
        {
            get
            { 
                if (this.powerdef != null)
                {
                    if (this.powerdef.MainVerb != null)
                    {
                        if (this.powerdef.MainVerb.SecondsToRecharge > 0)
                        {
                            return (int)(this.powerdef.MainVerb.SecondsToRecharge * GenTicks.TicksPerRealSecond);
                        }
                    }
                }
                return 120;
            }
        }

        //public void InitializePawnComps(Pawn parent)
        //{
        //    //           Log.Message("Initializng Pawn Comps");
        //    //           Log.Message(parent.ToString());
        //    for (int i = 0; i < this.powerdef.comps.Count; i++)
        //    {
        //        ThingComp thingComp = (ThingComp)Activator.CreateInstance(this.powerdef.comps[i].compClass);
        //        //              if (thingComp == null) Log.Message("NoTHingComp");
        //        thingComp.parent = parent;
        //        // if (this.comps == null) Log.Message("NoCompslist");

        //        thingComp.Initialize(this.powerdef.comps[i]);
        //        this.comps.Add(thingComp);
        //        parent.AllComps.Add(thingComp);
        //    }
        //}
        

        public void ExposeData()
        {
            //base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.TicksUntilCasting, "TicksUntilcasting", -1);
            Scribe_References.LookReference<Pawn>(ref this.pawn, "pawn");
            Scribe_Defs.LookDef<AbilityDef>(ref this.powerdef, "powerdef");
            //Scribe_Collections.LookList<ThingComp>(ref this.comps, "comps", LookMode.Undefined);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.PowerButton = powerdef.uiIcon;
            }
        }

    }
}



