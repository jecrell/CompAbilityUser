﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace AbilityUser
{
    /*
    "This class is primarily formed from code made by Cpt. Ohu for his Warhammer 40k mod.
     Credit goes where credit is due.
     Bless you, Ohu."
                                    -Jecrell
    */
    
    public class CompAbilityUser : CompUseEffect
    {
        
        //public AbilityPowerManager abilityPowerManager;
        public LocalTargetInfo CurTarget;
        public AbilityDef curPower;
        public Verb_UseAbility curVerb;
        public Rot4 curRotation;
        //public bool IsActive;
        public bool ShotFired = true;
        public bool IsInitialized = false;
        public float TicksToCastPercentage = 1;
        public int ticksToImpact = 500;
        public int TicksToCastMax = 100;
        public int TicksToCast = -1;

        public List<PawnAbility> Powers = new List<PawnAbility>();
        public List<PawnAbility> temporaryWeaponPowers = new List<PawnAbility>();
        public List<PawnAbility> temporaryApparelPowers = new List<PawnAbility>();
        public List<PawnAbility> allPowers = new List<PawnAbility>();
        public List<Verb_UseAbility> AbilityVerbs = new List<Verb_UseAbility>();
        public Dictionary<PawnAbility, Verb_UseAbility> pawnAbilities = new Dictionary<PawnAbility, Verb_UseAbility>();

        public void AddPawnAbility(AbilityDef abilityDef)
        {
            if (!this.Powers.Any(x => x.powerdef.defName == abilityDef.defName))
            {
                this.Powers.Add(new PawnAbility(this.abilityUser, abilityDef));
            }

            this.UpdateAbilities();
        }

        public void RemovePawnAbility(AbilityDef abilityDef)
        {
            PawnAbility abilityToRemove = this.Powers.FirstOrDefault(x => x.powerdef.defName == abilityDef.defName);
            if (abilityToRemove != null)
            {
                this.Powers.Remove(abilityToRemove);
            }

            this.UpdateAbilities();
        }

        public Pawn abilityUserSave = null;
        public Pawn abilityUser
        {
            get
            {
                if (abilityUserSave == null)
                {
                    abilityUserSave = this.parent as Pawn;
                }
                return abilityUserSave;
            }
        }
        public CompProperties_AbilityUser Props
        {
            get
            {
                return (CompProperties_AbilityUser)props;
            }
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!IsInitialized) Initialize();
            this.TicksToCast--;
            if (this.TicksToCast < -1)
            {
                //this.IsActive = true;
                this.ShotFired = true;
                this.TicksToCast = -1;
            }
            if (Powers != null && Powers.Count > 0)
            {
                foreach (PawnAbility power in Powers)
                {
                    power.PawnAbilityTick();
                }
            }
            this.TicksToCastPercentage = (1 - (this.TicksToCast / this.TicksToCastMax));
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }


            foreach (Command_Target comm in GetPawnAbilityVerbs().ToList())
            {
                yield return comm;
            }

        }

        public override void PostExposeData()
        {
            //base.PostExposeData();
            Scribe_Collections.LookList<PawnAbility>(ref this.allPowers, "allPowers", LookMode.Deep, new object[]
                {
                    this,
                });
            Scribe_Collections.LookList<PawnAbility>(ref this.temporaryApparelPowers, "temporaryApparelPowers", LookMode.Deep, new object[]
                {
                    this,
                });
            Scribe_Collections.LookList<PawnAbility>(ref this.temporaryWeaponPowers, "temporaryWeaponPowers", LookMode.Deep, new object[]
                {
                    this,
                });
            Scribe_Collections.LookList<PawnAbility>(ref this.Powers, "Powers", LookMode.Deep, new object[]
                {
                    this,
                });

            Scribe_Values.LookValue<int>(ref this.TicksToCast, "TicksToCast", 0, false);
            Scribe_Values.LookValue<int>(ref this.TicksToCastMax, "TicksToCastMax", 1, false);
            Scribe_Values.LookValue<float>(ref this.TicksToCastPercentage, "TicksToCastPercentage", 1, false);
            //Scribe_Values.LookValue<bool>(ref this.IsActive, "IsActive", false, false);
            Scribe_Values.LookValue<bool>(ref this.ShotFired, "ShotFired", true, false);
            Scribe_Values.LookValue<bool>(ref this.IsInitialized, "IsInitialized", false);
            Log.Message("PostExposeData Called: AbilityUser");
        }

        #region virtual

        public virtual void PostInitialize() { }

        public virtual void Initialize()
        {
            IsInitialized = true;
            //this.abilityPowerManager = new AbilityPowerManager(this);
            PostInitialize();
        }

        public virtual List<HediffDef> ignoredHediffs()
        {
            List<HediffDef> result = new List<HediffDef>();
            return result;
        }


        public virtual bool CanCastPowerCheck(Verb_UseAbility verbAbility, out string reason)
        {
            reason = "";
            return true;
        }

        public virtual void PostCastAbilityEffects(Verb_UseAbility verb)
        {

        }
        #endregion virtual

        public void UpdateAbilities()
        {

            AbilityVerbs.Clear();
            List<PawnAbility> abList = new List<PawnAbility>();

            abList.AddRange(this.Powers);

            abList.AddRange(this.temporaryWeaponPowers);

            this.allPowers.Clear();

            this.allPowers = abList;

            for (int i = 0; i < allPowers.Count; i++)
            {
                Verb_UseAbility newVerb = (Verb_UseAbility)Activator.CreateInstance(abList[i].powerdef.MainVerb.verbClass);
                if (!AbilityVerbs.Any(item => item.verbProps == newVerb.verbProps))
                {
                    //Log.Message("UpdateAbilities: Added to AbilityVerbs");
                    newVerb.caster = this.abilityUser;
                    newVerb.ability = abList[i];
                    newVerb.verbProps = abList[i].powerdef.MainVerb;
                    AbilityVerbs.Add(newVerb);
                }
            }

            this.pawnAbilities.Clear();

            foreach (PawnAbility pow in abList)
            {
                Verb_UseAbility newVerb = (Verb_UseAbility)Activator.CreateInstance(pow.powerdef.MainVerb.verbClass);
                if (!AbilityVerbs.Any(item => item.verbProps == newVerb.verbProps))
                {
                    //Log.Message("UpdateAbilities: Added to pawnAbilities");
                    newVerb.caster = this.abilityUser;
                    newVerb.ability = pow;
                    newVerb.verbProps = pow.powerdef.MainVerb;
                    pawnAbilities.Add(pow, newVerb);
                }
            }
            //       Log.Message(this.PawnAbilitys.Count.ToString());
        }



        public IEnumerable<Command_PawnAbility> GetPawnAbilityVerbs()
        {
            //Log.ErrorOnce("GetPawnAbilityVerbs Called", 912912);
            List<Verb_UseAbility> temp = new List<Verb_UseAbility>();
            temp.AddRange(this.AbilityVerbs);
            for (int i = 0; i < allPowers.Count; i++)
            {
                int j = i;
                Verb_UseAbility newVerb = temp[j];
                VerbProperties_Ability newVerbProps = newVerb.useAbilityProps;
                newVerb.caster = this.abilityUser;
                newVerb.verbProps = temp[j].verbProps;

                Command_PawnAbility command_CastPower = new Command_PawnAbility(this, allPowers[i]);
                command_CastPower.verb = newVerb;
                command_CastPower.defaultLabel = allPowers[j].powerdef.LabelCap;


                //GetDesc
                StringBuilder s = new StringBuilder();
                s.AppendLine(allPowers[j].powerdef.GetDescription());
                s.AppendLine(PostAbilityVerbCompDesc(newVerb));
                command_CastPower.defaultDesc = s.ToString();
                s = null;


                command_CastPower.targetingParams = allPowers[j].powerdef.MainVerb.targetParams;
                //command_CastPower.targetingParams = TargetingParameters.ForAttackAny();

                //if (newVerb.useAbilityProps.AbilityTargetCategory == AbilityTargetCategory.TargetSelf)
                //{
                //    command_CastPower.targetingParams = TargetingParameters.ForSelf(this.abilityUser);
                //}
                //else
                //{
                //    command_CastPower.targetingParams = TargetingParameters.
                //}
                command_CastPower.icon = allPowers[j].powerdef.uiIcon;
                string str;
                if (FloatMenuUtility.GetAttackAction(this.abilityUser, LocalTargetInfo.Invalid, out str) == null)
                {
                    command_CastPower.Disable(str.CapitalizeFirst() + ".");
                }
                command_CastPower.action = delegate (Thing target)
                {
                    Action attackAction = CompAbilityUser.TryCastAbility(abilityUser, target, this, newVerb, allPowers[j].powerdef as AbilityDef);
                    if (attackAction != null)
                    {
                        attackAction();
                        PostCastAbilityEffects(newVerb); //Another hook for modders
                    }
                };
                if (newVerb.caster.Faction != Faction.OfPlayer)
                {
                    command_CastPower.Disable("CannotOrderNonControlled".Translate());
                }
                string reason = "";
                if (newVerb.CasterIsPawn)
                {
                    if (newVerb.CasterPawn.story.DisabledWorkTags.Contains(WorkTags.Violent) && newVerbProps.isViolent)
                    {
                        command_CastPower.Disable("IsIncapableOfViolence".Translate(new object[]
                        {
                            newVerb.CasterPawn.NameStringShort
                        }));
                    }
                    else if (!newVerb.CasterPawn.drafter.Drafted)
                    {
                        command_CastPower.Disable("IsNotDrafted".Translate(new object[]
                        {
                            newVerb.CasterPawn.NameStringShort
                        }));
                    }
                    else if (!newVerb.ability.CanFire)
                    {
                        command_CastPower.Disable("AU_PawnAbilityRecharging".Translate(new object[]
                            {
                                newVerb.CasterPawn.NameStringShort
                            }));
                    }
                    //This is a hook for modders.
                    else if (!CanCastPowerCheck(newVerb, out reason))
                        {
                            command_CastPower.Disable(reason.Translate(new object[]
                            {
                        newVerb.CasterPawn.NameStringShort
                            }));
                        }
                }
                yield return command_CastPower;
            }
            temp = null;
            yield break;
        }

        public virtual string PostAbilityVerbCompDesc(Verb_UseAbility verb)
        {
            return "";
        }


        public static Job AbilityJob(AbilityTargetCategory cat, LocalTargetInfo target)
        {
            switch (cat)
            {
                case AbilityTargetCategory.TargetSelf:
                    {
                        return new Job(AbilityDefOf.CastAbilitySelf, target);
                    }
                case AbilityTargetCategory.TargetAoE:
                    {
                        return new Job(AbilityDefOf.CastAbilitySelf, target);
                    }
                case AbilityTargetCategory.TargetThing:
                    {
                        return new Job(AbilityDefOf.CastAbilityVerb, target);
                    }
                default:
                    {
                        return new Job(AbilityDefOf.CastAbilityVerb, target);
                    }
            }
        }


        public static Action TryCastAbility(Pawn pawn, LocalTargetInfo target, CompAbilityUser compAbilityUser, Verb_UseAbility verb, AbilityDef psydef)
        {

            Action act = new Action(delegate
            {
                compAbilityUser.CurTarget = null;
                compAbilityUser.CurTarget = target;
                compAbilityUser.curVerb = verb;
                compAbilityUser.curPower = psydef;
                compAbilityUser.curRotation = Rot4.South;
                if (target.Thing != null)
                {
                    compAbilityUser.curRotation = target.Thing.Rotation;
                }

                Job job = CompAbilityUser.AbilityJob(verb.useAbilityProps.AbilityTargetCategory, target);
                job.playerForced = true;
                job.verbToUse = verb;
                Pawn pawn2 = target.Thing as Pawn;
                if (pawn2 != null)
                {
                    job.killIncappedTarget = pawn2.Downed;
                }
                pawn.jobs.TryTakeOrderedJob(job);
            });
            return act;
        }

 

    }

}
