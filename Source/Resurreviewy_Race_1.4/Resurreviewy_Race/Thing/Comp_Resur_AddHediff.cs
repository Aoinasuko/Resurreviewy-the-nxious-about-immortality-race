using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Resurreviewy_Race
{
	public class CompProperties_AddHediff : CompProperties
	{
		public float Range;

		public HediffDef Hediff;

		public bool Enemy = false;

		public CompProperties_AddHediff()
		{
			compClass = typeof(Comp_AddHediff);
		}
	}

	public class Comp_AddHediff : ThingComp
	{
		public CompProperties_AddHediff Props => (CompProperties_AddHediff)props;
		public override void CompTick()
		{
			if (this.parent.IsHashIntervalTick(300))
			{
				CompRefuelable fuel = this.parent.GetComp<CompRefuelable>();
				CompFlickable forb = this.parent.GetComp<CompFlickable>();
				if (fuel.HasFuel)
				{
					if (forb.SwitchIsOn)
                    {
						Map map = this.parent.Map;
						if (map != null)
						{
							IEnumerable<Pawn> pawns;
							if (Props.Enemy == false)
							{
								pawns = map.mapPawns.AllPawnsSpawned.Where(x => x.Position.DistanceTo(this.parent.Position) <= Props.Range && this.parent.Faction == x.Faction);
							}
							else
							{
								pawns = map.mapPawns.AllPawnsSpawned.Where(x => x.Position.DistanceTo(this.parent.Position) <= Props.Range && x.HostileTo(this.parent));
							}
							if (!pawns.EnumerableNullOrEmpty())
							{
								foreach (Pawn pawn in pawns)
								{
									pawn.health.AddHediff(Props.Hediff);
								}
							}
						}
					}
				}
			}
		}
	}
}
