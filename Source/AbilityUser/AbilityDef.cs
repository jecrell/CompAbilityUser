using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AbilityUser
{
    public class AbilityDef : ThingDef
    {
        public int RechargeTicks;

        public int CastTime = 0;

        public VerbProperties_Ability MainVerb;



        public virtual string GetDescription()
        {
            string result = "";
            string coolDesc = GetBasics();
            string AoEDesc = GetAoEDesc();
            //string postDesc = PostAbilityVerbDesc();
            StringBuilder desc = new StringBuilder();
            desc.AppendLine(this.description);
            if (coolDesc != "") desc.AppendLine(coolDesc);
            if (AoEDesc != "") desc.AppendLine(AoEDesc);
            //if (postDesc != "") desc.AppendLine(postDesc);
            result = desc.ToString();
            return result;
        }

        public virtual string GetAoEDesc()
        {
            string result = "";
            VerbProperties_Ability def = MainVerb;
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

        public string GetBasics()
        {
            string result = "";
            VerbProperties_Ability def = MainVerb;
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
    }
}
