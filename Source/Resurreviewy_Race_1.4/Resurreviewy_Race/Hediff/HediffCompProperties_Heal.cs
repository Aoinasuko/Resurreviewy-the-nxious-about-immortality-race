using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Resurreviewy_Race
{
	public class HediffCompProperties_Heal : HediffCompProperties
	{
		public bool smallheal;

		public bool bigheal;

		public int smallhealtime;

		public int bighealtime;

		public HediffCompProperties_Heal()
		{
			compClass = typeof(HediffComp_Heal);
		}
	}

	public class HediffComp_Heal : HediffComp
	{
		public HediffCompProperties_Heal Props => (HediffCompProperties_Heal)props;

		int smallhealtime = 500;

		int bighealtime = 2500;

		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.Look(ref smallhealtime, "smallhealtime", 0);
			Scribe_Values.Look(ref bighealtime, "bighealtime", 0);
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			if (Pawn.Spawned)
			{
				if (Props.smallheal)
				{
					smallhealtime--;
				}
				if (Props.bigheal)
				{
					bighealtime--;
				}
				if (smallhealtime <= 0)
				{
					Util.HealInjury(Pawn, 2);
					smallhealtime = Props.smallhealtime;
				}
				if (bighealtime <= 0)
				{
					Util.HealInjury(Pawn, 10);
					Util.HealFirstMissingBodyPart(Pawn);
					Util.HealFirstDisease(Pawn);
					bighealtime = Props.bighealtime;
				}
			}
		}
	}
}
