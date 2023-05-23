using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Resurreviewy_Race
{
	public class Resurreviewy_WorldComponent : WorldComponent
	{

		public Resurreviewy_WorldComponent(World world)
			: base(world)
		{
		}

		public override void WorldComponentTick()
		{
			int ticks = Find.TickManager.TicksAbs;
			if (ticks % 600 == 0)
			{
				// 現在のバージョン
				if (Resurreviewy_Config.Updatever < Resurreviewy_Config.ver)
				{
					Resurreviewy_Config.Updatever = Resurreviewy_Config.ver;
					Dialog_Update dialog = new Dialog_Update();
					Find.WindowStack.Add(dialog);
					LoadedModManager.GetMod<Resurreviewy_Settings>().WriteSettings();
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
		}
	}

	public class Dialog_Update : Window
	{
		public override Vector2 InitialSize => new Vector2(700f, 700f);

		public Dialog_Update()
		{
			forcePause = true;
			doCloseX = true;
			absorbInputAroundWindow = true;
			closeOnAccept = false;
			closeOnClickedOutside = false;
		}

		private static Vector2 scrollPosition;

		public override void DoWindowContents(Rect inRect)
		{
			Texture2D texture_update = ContentFinder<Texture2D>.Get("Resur/UI/Update");
			Texture2D texture_nasuko = ContentFinder<Texture2D>.Get("Resur/UI/Resarry");

			Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height + 3000f);
			Rect outRect = new Rect(0f, 30f, inRect.width, inRect.height - 80f);
			// 通常設定
			Listing_Standard listingStandard = new Listing_Standard();
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			listingStandard.maxOneColumn = true;
			listingStandard.ColumnWidth = viewRect.width / 1.1f;
			listingStandard.Begin(viewRect);
			listingStandard.Gap(10f);
			listingStandard.Label("Resur.Update.Title".Translate());
			listingStandard.GapLine();
			listingStandard.Gap(100f);
			Widgets.DrawTextureFitted(new Rect(0, listingStandard.CurHeight, viewRect.width, 200f), texture_update, 1.5f);
			listingStandard.Gap(250f);
			listingStandard.Label("Resur.Update.label".Translate());
			listingStandard.End();
			Widgets.DrawTextureFitted(new Rect(viewRect.width / 8 * 7, listingStandard.CurHeight, viewRect.width / 8, 50), texture_nasuko, 1.0f);
			Widgets.EndScrollView();
		}

	}
}
