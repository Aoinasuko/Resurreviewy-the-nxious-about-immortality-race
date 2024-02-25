using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Resurreviewy_Race
{
    public class CompProperties_Assistance : CompProperties
    {
        public CompProperties_Assistance()
        {
            compClass = typeof(CompAssistance);
        }
    }

    [StaticConstructorOnStartup]
    public class CompAssistance : ThingComp
    {
        public CompProperties_Assistance Props => (CompProperties_Assistance)props;

        public override void CompTick()
        {
            if (this.parent.Map == null)
            {
                return;
            }
            if (this.parent.IsHashIntervalTick(180))
            {
                if (isactive())
                {
                    if (Find.ResearchManager.currentProj != null)
                    {
                        // プロジェクトの進捗を進める
                        ResearchProjectDef proj = Find.ResearchManager.currentProj;
                        if (proj.ProgressReal < proj.CostApparent - 1.0f)
                        {
                            Find.ResearchManager.AddResearchDirect();
                        }
                    }
                }
            }
        }

        private bool isactive()
        {
            CompPowerTrader power = this.parent.GetComp<CompPowerTrader>();
            if (power != null)
            {
                if (power.PowerOn)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
