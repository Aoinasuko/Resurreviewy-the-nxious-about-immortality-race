using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Resurreviewy_Race
{
	public class CompProperties_BloodCollecter : CompProperties
	{
		public CompProperties_BloodCollecter()
		{
			compClass = typeof(Comp_BloodCollecter);
		}
	}

	public class Comp_BloodCollecter : ThingComp
	{
		float blood = 0f;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref blood, "blood", 0f);
        }

        public CompProperties_BloodCollecter Props => (CompProperties_BloodCollecter)props;
		public override void CompTick()
		{
			if (this.parent.IsHashIntervalTick(300))
			{
				CompRefuelable fuel = this.parent.GetComp<CompRefuelable>();
				CompFlickable forb = this.parent.GetComp<CompFlickable>();
				if (fuel.HasFuel && fuel.Fuel >= 0.2f)
				{
					if (forb.SwitchIsOn)
                    {
						Map map = this.parent.Map;
						if (map != null)
						{
							int count = map.mapPawns.AllPawnsSpawned.Where(x => (x.def.defName == "Resurreviewy_Pawn" || x.def.defName == "Resurrevi") && x.Position.DistanceTo(this.parent.Position) <= 10.9f).Count();
							if (count > 0)
							{
								blood += 0.01f * count;
								if (blood >= 1.00f)
								{
									fuel.ConsumeFuel(0.2f);
                                    GenSpawn.Spawn(ThingDef.Named("Resur_BloodSyringe"), this.parent.InteractionCell, this.parent.Map);
									blood = 0.00f;
                                }
                            }
                        }
					}
				}
			}
		}

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Resur.Build.Blood".Translate() + this.blood.ToStringPercent());
            return stringBuilder.ToString();
        }
    }
}
