using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Resurreviewy_Race
{
	// リザレビューイにのみ使用できる
	public class CompTargetable_UseResur : CompTargetable
	{
		protected override bool PlayerChoosesTarget => true;

		protected override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = false,
				validator = ((TargetInfo x) => BaseTargetValidator(x.Thing) && (x.Thing?.def.defName == "Resurreviewy_Pawn" || x.Thing?.def.defName == "Resurrevi"))
			};
		}

		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			yield return targetChosenByPlayer;
		}
	}

	public class CompProperties_Resur_UseItem : CompProperties
	{
		public ThingDef moteDef;

		public CompProperties_Resur_UseItem()
		{
			compClass = typeof(CompTargetEffect_Resur_UseItem);
		}
	}

	public class CompTargetEffect_Resur_UseItem : CompTargetEffect
	{
		public CompProperties_Resur_UseItem Props => (CompProperties_Resur_UseItem)props;

		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (user.IsColonistPlayerControlled && user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
			{
				Job job = JobMaker.MakeJob(Job_Resur.Resur_UseItem, target, parent);
				job.count = 1;
				user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}
	}

	// アイテムの使用
	public class JobDriver_UseItem : JobDriver
	{
		private const int DurationTicks = 300;

		private Mote warmupMote;

		private Pawn TargetPawn => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		private Thing Item => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(TargetPawn, job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil toil = Toils_General.Wait(300);
			toil.initAction = delegate
			{
				if (pawn != TargetPawn)
                {
					PawnUtility.ForceWait(TargetPawn, 300, null, true);
				}
			};
			toil.WithProgressBarToilDelay(TargetIndex.A);
			toil.FailOnDespawnedOrNull(TargetIndex.A);
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			toil.tickAction = delegate
			{
				CompUsable compUsable = Item.TryGetComp<CompUsable>();
				if (compUsable != null && warmupMote == null && compUsable.Props.warmupMote != null)
				{
					warmupMote = MoteMaker.MakeAttachedOverlay(TargetPawn, compUsable.Props.warmupMote, Vector3.zero);
				}
				warmupMote?.Maintain();				
			};
			yield return toil;
			yield return Toils_General.Do(Apply);
		}

		private void Apply()
		{
			SoundDefOf.MechSerumUsed.PlayOneShot(SoundInfo.InMap(TargetPawn));
			ThingDef thingDef = Item?.TryGetComp<CompTargetEffect_Resur_UseItem>()?.Props.moteDef;
			if (thingDef != null)
			{
				MoteMaker.MakeAttachedOverlay(TargetPawn, thingDef, Vector3.zero);
			}
			if (Item.def.defName == "Resur_ElixirofEternalLife")
            {
				if (TargetPawn.health.hediffSet.HasHediff(Hediff_Resur.Resur_Antidote_EternalLife))
                {
					TargetPawn.health.RemoveHediff(TargetPawn.health.hediffSet.GetFirstHediffOfDef(Hediff_Resur.Resur_Antidote_EternalLife));
				}
				if (TargetPawn.def.defName != "Resurrevi" && TargetPawn.def.defName != "Resurreviewy_Pawn")
                {
					TargetPawn.health.AddHediff(Hediff_Resur.Resur_EternalLife);
				}
				Item.SplitOff(1).Destroy();
				return;
            }
			if (Item.def.defName == "Resur_AntidoteofEternalLife")
			{
				if (TargetPawn.health.hediffSet.HasHediff(Hediff_Resur.Resur_EternalLife))
				{
					TargetPawn.health.RemoveHediff(TargetPawn.health.hediffSet.GetFirstHediffOfDef(Hediff_Resur.Resur_EternalLife));
				}
				if (TargetPawn.def.defName == "Resurrevi" || TargetPawn.def.defName == "Resurreviewy_Pawn")
				{
					TargetPawn.health.AddHediff(Hediff_Resur.Resur_Antidote_EternalLife);
				}
				Item.SplitOff(1).Destroy();
				return;
			}
			if (Item?.TryGetComp<comp_Healer>() == null)
            {
				Thing blood = GenSpawn.Spawn(ThingDef.Named("Resur_BloodSyringe"), TargetPawn.Position, TargetPawn.Map);
				blood.stackCount = 1;
			} else
            {
				Item?.TryGetComp<comp_Healer>().heal(TargetPawn);
			}
			Item.SplitOff(1).Destroy();
		}
	}

}
