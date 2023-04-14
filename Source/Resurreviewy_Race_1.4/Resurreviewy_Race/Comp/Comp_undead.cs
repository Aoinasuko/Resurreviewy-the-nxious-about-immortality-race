using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Resurreviewy_Race
{
	public class CompProperties_Undead : CompProperties
	{
		public int smallhealtime = 600;

		public int bighealtime = 3000;

		public CompProperties_Undead()
		{
			compClass = typeof(CompUndead);
		}
	}

	public class CompUndead : ThingComp
	{
		int smallhealtime = 600;

		int bighealtime = 3000;

		bool downflag = false;

		public CompProperties_Undead Props => (CompProperties_Undead)props;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref smallhealtime, "smallhealtime", 0);
			Scribe_Values.Look(ref bighealtime, "bighealtime", 0);
			Scribe_Values.Look(ref downflag, "downflag", false);
		}

		public override void CompTick()
		{
			if (parent.Spawned)
			{
				bool isDown = false;
				Pawn pawn = parent as Pawn;
				if (pawn.Faction?.IsPlayer ?? false)
				{
					smallhealtime--;
					bighealtime--;
				}
				else
				{
					if (pawn.IsHashIntervalTick(4))
					{
						smallhealtime--;
						bighealtime--;
					}
					if (pawn.health.hediffSet.HasHediff(HediffDef.Named("Resur_RegenerationInhibition")))
                    {
						isDown = true;
					} else
                    {
						if (pawn.Downed && downflag == false)
                        {
							pawn.health.AddHediff(HediffDef.Named("Resur_RegenerationInhibition"));
							downflag = true;
							isDown = true;
						}
                    }
				}
				if (!pawn.Downed)
				{
					downflag = false;
				}
				if (smallhealtime <= 0)
				{
					if (!isDown)
					{
						Util.HealInjury(pawn, 5);
					}
					smallhealtime = Props.smallhealtime;
				}
				if (bighealtime <= 0)
				{
					if (Util.isundead(pawn))
					{
						if (!isDown)
						{
							Util.HealInjury(pawn, 99999);
							Util.HealFirstMissingBodyPart(pawn);
							Util.HealFirstDisease(pawn);
							if (pawn.needs.food.CurLevel == 0.00f)
							{
								pawn.TakeDamage(new DamageInfo(DamageDefOf.Bite, 10.0f, 50.0f, -1, pawn, null, null));
								pawn.needs.food.CurLevel += 0.35f;
							}
						}
					}
					bighealtime = Props.bighealtime;
				}
			}
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
		{
			if (parent.def.defName != "Resurreviewy_Pawn" && parent.def.defName != "Resurrevi")
            {
				yield break;
			}
			Pawn pawn = parent as Pawn;
			if (!pawn.Downed || (pawn.Faction?.IsPlayer ?? false))
			{
				yield break;
			}
			if (!myPawn.CanReach(pawn, PathEndMode.Touch, Danger.Deadly))
			{
				yield return new FloatMenuOption("Resur.UI.CantBuried".Translate() + " (" + "NoPath".Translate() + ")", null);
				yield break;
			}
			if (!myPawn.CanReserve(pawn, 1, -1, null))
			{
				String text = "Resur.UI.Buried".Translate(pawn);
				text = ((pawn == null) ? ((string)(text + (" (" + "Reserved".Translate() + ")"))) : ((string)(text + (" (" + "ReservedBy".Translate(myPawn.LabelShort, myPawn) + ")"))));
				yield return new FloatMenuOption(text, null);
				yield break;
			} else
            {
				Action action_job = delegate ()
				{
					JobDef jobdef = Job_Resur.Resur_Buried;
					Job job = new Job(jobdef, pawn);
					job.count = 1;
					myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				};
				String text = "Resur.UI.Buried".Translate(pawn);
				yield return new FloatMenuOption(text, action_job);
				yield break;
			}
		}
	}
}
