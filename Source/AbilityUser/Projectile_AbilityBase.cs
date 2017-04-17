using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AbilityUser
{
    public class Projectile_AbilityBase : Projectile
    {
        public List<ExtraDamage> extraDamages = null;
        public List<ApplyHediffs> localApplyHediffs = null;
        public List<ApplyMentalStates> localApplyMentalStates = null;

        public Vector3 targetVec;
        public Pawn Caster;
        public Thing selectedTarget;

        /// <summary>
        /// Applies damage on a collateral pawn or an object.
        /// </summary>
        protected void ApplyDamage(Thing hitThing)
        {
            if (hitThing != null)
            {
                // Impact collateral target.
                this.Impact(hitThing);
            }
            else
            {
                this.ImpactSomething();
            }
        }
        
        /// <summary>
        /// Computes what should be impacted in the DestinationCell.
        /// </summary>
        protected void ImpactSomething()
        {
            // Check impact on a thick mountain.
            if (this.def.projectile.flyOverhead)
            {
                RoofDef roofDef = this.Map.roofGrid.RoofAt(this.DestinationCell);
                if (roofDef != null && roofDef.isThickRoof)
                {
                    SoundInfo info = SoundInfo.InMap(new TargetInfo(this.DestinationCell, this.Map, false), MaintenanceType.None);
                    this.def.projectile.soundHitThickRoof.PlayOneShot(info);
                    return;
                }
            }

            // Impact the initial targeted pawn.
            if (this.assignedTarget != null)
            {
                Pawn pawn = this.assignedTarget as Pawn;
                if (pawn != null && pawn.Downed && (this.origin - this.destination).magnitude > 5f && Rand.Value < 0.2f)
                {
                    this.Impact(null);
                    return;
                }
                this.Impact(this.assignedTarget);
                return;
            }
            else
            {
                // Impact a pawn in the destination cell if present.
                Thing thing = this.Map.thingGrid.ThingAt(this.DestinationCell, ThingCategory.Pawn);
                if (thing != null)
                {
                    this.Impact(thing);
                    return;
                }
                // Impact any cover object.
                foreach (Thing current in this.Map.thingGrid.ThingsAt(this.DestinationCell))
                {
                    if (current.def.fillPercent > 0f || current.def.passability != Traversability.Standable)
                    {
                        this.Impact(current);
                        return;
                    }
                }
                this.Impact(null);
                return;
            }
        }

        public ProjectileDef_Ability mpdef
        {
            get
            {
                ProjectileDef_Ability mpdef = null;
                if (def is ProjectileDef_Ability)
                {
                    mpdef = def as ProjectileDef_Ability;
                }
                return mpdef;
            }
        }


        public virtual bool CanOverpower(Pawn caster, Thing hitThing)
        {
            return true;
        }

        public virtual void Impact_Override(Thing hitThing)
        {
            //base.Impact(hitThing);
            Pawn victim = hitThing as Pawn;
            if (victim != null)
            {

                //Log.Message("1");
                if (mpdef != null)
                {

                    //Log.Message("2");
                    if (localApplyMentalStates != null && localApplyMentalStates.Count > 0)
                    {

                        //Log.Message("3");
                        foreach (ApplyMentalStates mentalStateGiver in localApplyMentalStates)
                        {
                            //Log.Message("4");
                            bool success = false;
                            float checkValue = Rand.Value;
                            Log.Message(checkValue.ToString());
                            Log.Message(mentalStateGiver.applyChance.ToString());
                            if (checkValue <= mentalStateGiver.applyChance)
                            {
                                string str = "MentalStateByPsyker".Translate(new object[]
                                 {
                            victim.NameStringShort,
                                 });
                                if (mentalStateGiver.mentalStateDef == MentalStateDefOf.Berserk && victim.RaceProps.intelligence < Intelligence.Humanlike)
                                {
                                    if (CanOverpower(this.Caster, victim))
                                    {
                                        success = true;
                                        victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, str, true);
                                    }
                                }
                                else
                                {
                                    if (CanOverpower(this.Caster, victim))
                                    {
                                        success = true;
                                        victim.mindState.mentalStateHandler.TryStartMentalState(mentalStateGiver.mentalStateDef, str, true);
                                    }
                                }
                            }
                            if (success)
                                MoteMaker.ThrowText(this.Caster.PositionHeld.ToVector3(), this.Caster.MapHeld, "CastSuccess".Translate(), 12f);
                            else
                                MoteMaker.ThrowText(this.Caster.PositionHeld.ToVector3(), this.Caster.MapHeld, "CastFailure".Translate(), 12f);
                        }
                    }
                    //else if (mpdef.IsBuffGiver && victim.needs.TryGetNeed<Need_Soul>().PsykerPowerLevel != PsykerPowerLevel.Omega)
                    if (localApplyHediffs != null && localApplyHediffs.Count > 0)
                    {
                        foreach (ApplyHediffs hediffs in localApplyHediffs)
                        {
                            bool success = false;
                            if (Rand.Value <= hediffs.applyChance)
                            {
                                if (CanOverpower(this.Caster, victim))
                                {
                                    victim.health.AddHediff(hediffs.hediffDef);
                                    success = true;
                                }
                            }
                            if (success)
                                MoteMaker.ThrowText(this.Caster.PositionHeld.ToVector3(), this.Caster.MapHeld, "Success".Translate());
                            else
                                MoteMaker.ThrowText(this.Caster.PositionHeld.ToVector3(), this.Caster.MapHeld, "Failed".Translate());
                        }
                    }
                }
            }
        }

        // Verse.Projectile
        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing equipment = null, List<ApplyHediffs> applyHediffs = null, List<ApplyMentalStates> applyMentalStates = null)
        {
            localApplyHediffs = applyHediffs;
            localApplyMentalStates = applyMentalStates;
            base.Launch(launcher, origin, targ, equipment);
        }

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            Impact_Override(hitThing);
            if (hitThing != null)
            {
                if (extraDamages != null && extraDamages.Count > 0)
                {
                    foreach (ExtraDamage damage in extraDamages)
                    {
                        DamageInfo extraDinfo = new DamageInfo(damage.damageDef, damage.damage, this.ExactRotation.eulerAngles.y, this.launcher, null, equipmentDef);
                        hitThing.TakeDamage(extraDinfo);
                    }
                }
            }
        }

    }
}
