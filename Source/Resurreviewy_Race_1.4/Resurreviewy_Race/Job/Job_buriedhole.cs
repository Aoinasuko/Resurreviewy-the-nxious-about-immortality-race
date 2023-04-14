using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Resurreviewy_Race
{
    public class JobDriver_Buriedhole : JobDriver
    {
        private float Progress;

        protected float WorkTotal = 100f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.Progress, "Progress", 0f, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.job.GetTarget(TargetIndex.A);
            Job job = this.job;
            return ReservationUtility.Reserve(pawn, target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ToilFailConditions.FailOnDespawnedNullOrForbidden(this, TargetIndex.A);
            ToilFailConditions.FailOnNotCasualInterruptible(this, TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil wait = new Toil();
            wait.initAction = delegate ()
            {
                Pawn actor = wait.actor;
                Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
                actor.pather.StopDead();
                PawnUtility.ForceWait(pawn, 15000, null, true);
            };
            wait.tickAction = delegate ()
            {
                Pawn actor = wait.actor;
                if (actor.IsHashIntervalTick(60))
                {
                    Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
                    SoundDef.Named("Tunnel_End").PlayOneShot(SoundInfo.InMap(pawn));
                    for (int i = 0; i < 3; i++)
                    {
                        FleckMaker.ThrowDustPuff(pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), pawn.Map, Rand.Range(0.8f, 1.2f));
                    }
                }
                if (actor.skills.GetSkill(SkillDefOf.Mining).TotallyDisabled)
                {
                    Progress += 0.03f;
                } else
                {
                    actor.skills.Learn(SkillDefOf.Mining, 0.13f, false);
                    Progress += StatExtension.GetStatValue(actor, StatDefOf.MiningSpeed, true);
                }                
                if (Progress >= WorkTotal)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                }
            };
            wait.AddFinishAction(delegate ()
            {
                Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
                pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                pawn.Destroy();
            });
            ToilFailConditions.FailOnDespawnedOrNull<Toil>(wait, TargetIndex.A);
            ToilFailConditions.FailOnCannotTouch<Toil>(wait, TargetIndex.A, PathEndMode.Touch);
            wait.AddEndCondition(delegate ()
            {
                return JobCondition.Ongoing;
            });
            wait.defaultCompleteMode = ToilCompleteMode.Never;
            ToilEffects.WithProgressBar(wait, TargetIndex.A, () => this.Progress / this.WorkTotal, false, -0.5f);
            wait.activeSkill = (() => SkillDefOf.Mining);
            yield return wait;
            yield break;
        }
    }

}
