using BEPRace_Core;
using HarmonyLib;
using RimWorld;
using RuntimeAudioClipLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Resurreviewy_Race
{
	/**
	 * フェイズティーバル
	 */
	public class HediffCompProperties_Drug_A : HediffCompProperties
	{
		public HediffCompProperties_Drug_A()
		{
			compClass = typeof(HediffComp_Drug_A);
		}
	}

	public class HediffComp_Drug_A : HediffComp
	{
        private Action<int> _delegate;

        public HediffCompProperties_Drug_A Props => (HediffCompProperties_Drug_A)props;

		public override void CompExposeData()
		{
			base.CompExposeData();
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			if (Pawn.IsHashIntervalTick(300))
			{
				// もし輝きの遺伝子を持っている場合、輝きを1回復する
				if (ModLister.BiotechInstalled)
				{
                    Gene_Brilliance gene = Pawn.genes.GetFirstGeneOfType<Gene_Brilliance>();
                    if (gene != null)
                    {
						gene.Value += 0.010f;
                    }
                }
                // リトルフェアリーの場合、FPを追加で1回復する
                if (Pawn.def.defName == "LittleFairy_Pawn")
				{
                    if (_delegate == null)
                    {
                        Type type = null;
                        MethodInfo method = null;
                        ThingComp comp = null;
                        try
                        {
                            comp = Pawn.AllComps.Where(x => x.GetType().FullName == "LittleFairy_Race.Comp_LitF").First();
                            type = comp.GetType();
                            method = type.GetMethod("FPHeal",
                            BindingFlags.Public | BindingFlags.Instance,
                            null,
                            new[] { typeof(int) },
                            null);
                        }
                        catch (Exception e)
                        {
                        }
                        if (method != null)
                        {
                            // デリゲートを生成
                            _delegate = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), comp, method);
                        }
                    }
                    _delegate?.Invoke(3);
                }
			}
		}
	}

    /**
	 * モアミルキー
	 */
    public class HediffCompProperties_Drug_B : HediffCompProperties
    {
        public HediffCompProperties_Drug_B()
        {
            compClass = typeof(HediffComp_Drug_B);
        }
    }

    public class HediffComp_Drug_B : HediffComp
    {
        public HediffCompProperties_Drug_B Props => (HediffCompProperties_Drug_B)props;

        public override void CompExposeData()
        {
            base.CompExposeData();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(300))
            {
                foreach (ThingComp comp in Pawn.AllComps)
                {
                    if (comp as CompHasGatherableBodyResource != null)
                    {
                        CompHasGatherableBodyResource compfix = comp as CompHasGatherableBodyResource;
                        if (compfix.Fullness < 1.0f)
                        {
                            AccessTools.Field(compfix.GetType(), "fullness").SetValue(comp, Math.Min(1.0f, compfix.Fullness + 0.01f));
                        }
                    }
                }
            }
        }
    }

}
