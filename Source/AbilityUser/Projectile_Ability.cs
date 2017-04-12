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
    public class Projectile_Ability : Projectile_AbilityBase
    {

        public int TicksToImpact
        {
            get
            {
                return this.ticksToImpact;
            }
        }

        public Vector3 ProjectileDrawPos
        {
            get
            {
                if (selectedTarget != null)
                {
                    return selectedTarget.DrawPos;
                }
                else if (targetVec != null)
                {
                    return targetVec;
                }
                return this.ExactPosition;
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

        public override void Draw()
        {
            if (selectedTarget != null || targetVec != null)
            {
                Vector3 vector = this.ProjectileDrawPos;
                Vector3 distance = this.destination - this.origin;
                Vector3 curpos = this.destination - this.Position.ToVector3();
                float angle = 0f;
                Material mat = this.Graphic.MatSingle;
                Vector3 s = new Vector3(2.5f, 1f, 2.5f);
                Matrix4x4 matrix = default(Matrix4x4);
                vector.y = 3;
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, this.DrawPos, this.ExactRotation, this.def.DrawMatSingle, 0);
            }
            base.Comps_PostDraw();
        }

        public override void Impact_Override(Thing hitThing)
        {
            //base.Impact(hitThing);
            if (hitThing != null)
            {

                Pawn victim = hitThing as Pawn;
                if (victim != null)
                {
                    if (mpdef != null)
                    {
                        if (mpdef.IsMentalStateGiver)
                        {
                            string str = "MentalStateByPsyker".Translate(new object[]
                             {
                            victim.NameStringShort,
                             });
                            if (mpdef.InducesMentalState == MentalStateDefOf.Berserk && victim.RaceProps.intelligence < Intelligence.Humanlike)
                            {
                                if (CanOverpower(this.Caster, victim))
                                {
                                    victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, str, true);
                                }
                            }
                            else
                            {
                                if (CanOverpower(this.Caster, victim))
                                {
                                    victim.mindState.mentalStateHandler.TryStartMentalState(mpdef.InducesMentalState, str, true);
                                }
                            }
                        }
                        //else if (mpdef.IsBuffGiver && victim.needs.TryGetNeed<Need_Soul>().PsykerPowerLevel != PsykerPowerLevel.Omega)
                        else if (mpdef.IsBuffGiver)
                        {
                            if (mpdef.BuffDef.isBad)
                            {
                                if (CanOverpower(this.Caster, victim))
                                {
                                    victim.health.AddHediff(mpdef.BuffDef);
                                }
                            }
                            else
                            {
                                victim.health.AddHediff(mpdef.BuffDef);
                            }
                        }
                    }
                    int damageAmountBase = this.def.projectile.damageAmountBase;
                    ThingDef equipmentDef = this.equipmentDef;
                    DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, this.ExactRotation.eulerAngles.y, this.launcher, null, equipmentDef);
                    hitThing.TakeDamage(dinfo);
                    PostImpactEffects(hitThing);
                }
            }
            else
            {
                //SoundDefOf.PowerOffSmall.PlayOneShotOnCamera();
            }
        }

        public virtual void PostImpactEffects(Thing hitThing)
        {

        }

        public virtual bool IsInIgnoreHediffList(Hediff hediff)
        {
            if (hediff != null)
            {
                if (hediff.def != null)
                {
                    CompAbilityUser compAbility = Caster.TryGetComp<CompAbilityUser>();
                    if (compAbility != null)
                    {
                        if (compAbility.ignoredHediffs() != null)
                        {
                            if (compAbility.ignoredHediffs().Contains(hediff.def)) { Log.Message("IgnoreHediff Passed"); return true; }
                        }
                    }
                }
            }
            return false;
        }

        public virtual bool CanOverpower(Pawn caster, Thing hitThing)
        {
            return true;
        }
    }
}
