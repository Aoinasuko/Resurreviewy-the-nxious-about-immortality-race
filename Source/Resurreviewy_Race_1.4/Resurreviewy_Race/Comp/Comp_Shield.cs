using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Resurreviewy_Race
{

	[StaticConstructorOnStartup]
	public class Gizmo_Shield : Gizmo
	{
		public CompShield comp;

		private static readonly Texture2D FullNPBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.2f));

		private static readonly Texture2D EmptyNPBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

		public Gizmo_Shield()
		{
			Order = -100f;
		}

		public override float GetWidth(float maxWidth)
		{
			return 140f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(6f);
			Widgets.DrawWindowBackground(rect);
			Rect rect3 = rect2;
			rect3.height = rect.height / 2f;
			Text.Font = GameFont.Tiny;
			Widgets.Label(rect3, "Resur.UI.BeltHP".Translate());
			Rect rect4 = rect2;
			rect4.yMin = rect2.y + rect2.height / 2f;
			float fillPercent = comp.GetHP() / Mathf.Max(1f, comp.Props.HPMax);
			Widgets.FillableBar(rect4, fillPercent, FullNPBarTex, EmptyNPBarTex, doBorder: false);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect4, comp.GetHP().ToString() + "/" + comp.Props.HPMax.ToString());
			Text.Anchor = TextAnchor.UpperLeft;
			return new GizmoResult(GizmoState.Clear);
		}

	}

	public class CompProperties_Shield : CompProperties
    {
		// 起動時のHP
		public int HPMax = 1800;

		// 1tickに減るHP
		public int LossHP = 1;

		// 1ダメージ辺りに減るHP
		public int DamageHP = 1;

		// 阻害系
		public bool IsShield = true;

		// 追加するHediff
		public HediffDef AddHediff;

		public CompProperties_Shield()
        {
            compClass = typeof(CompShield);
        }
    }

	public class CompShield : ThingComp
	{
		public CompProperties_Shield Props => (CompProperties_Shield)props;

		private int ShieldHP = 1800;

		private bool ShieldActive = false;

		public ShieldState ShieldState
		{
			get
			{
				if (ShieldActive && ShieldHP > 0)
                {
					return ShieldState.Active;
                }
				return ShieldState.Disabled;
			}
		}

		protected bool ShouldDisplay
		{
			get
			{
				Pawn pawnOwner = PawnOwner;
				if (!pawnOwner.Spawned || pawnOwner.Dead || pawnOwner.Downed)
				{
					return false;
				}
				if (pawnOwner.InAggroMentalState)
				{
					return true;
				}
				if (pawnOwner.Drafted)
				{
					return true;
				}
				if (pawnOwner.Faction.HostileTo(Faction.OfPlayer) && !pawnOwner.IsPrisoner)
				{
					return true;
				}
				if (ModsConfig.BiotechActive && pawnOwner.IsColonyMech && Find.Selector.SingleSelectedThing == pawnOwner)
				{
					return true;
				}
				return false;
			}
		}

		protected Pawn PawnOwner
		{
			get
			{
				Apparel apparel;
				if ((apparel = (parent as Apparel)) != null)
				{
					return apparel.Wearer;
				}
				Pawn result;
				if ((result = (parent as Pawn)) != null)
				{
					return result;
				}
				return null;
			}
		}

		public int GetHP()
		{
			return ShieldHP;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ShieldHP, "ShieldHP", 0);
			Scribe_Values.Look(ref ShieldActive, "ShieldActive", false);
		}

		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			if (ShieldActive)
            {
				Gizmo_Shield gizmo_Shield = new Gizmo_Shield();
				gizmo_Shield.comp = this;
				yield return gizmo_Shield;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (PawnOwner == null)
			{
				ShieldHP = 0;
				ShieldActive = false;
				return;
			}
			if (ShieldActive && ShieldHP > 0)
            {
				ShieldHP -= Props.LossHP;
				if (parent.IsHashIntervalTick(10))
                {
					if (Props.AddHediff != null)
                    {
						PawnOwner.health.AddHediff(Props.AddHediff);
					}
                }
				if (ShieldHP <= 0)
                {
					Break();
                }
			}
		}

		public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
			if (ShieldState != ShieldState.Active)
			{
				return;
			}
			else
			{
				if (!Props.IsShield)
                {
					return;
                }
				if (Props.DamageHP <= 0)
                {
					return;
                }
				if (dinfo.Def == DamageDefOf.EMP)
                {
					ShieldHP -= Props.HPMax / 5;
				} else
                {
					if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
					{
						ShieldHP -= (int)dinfo.Amount * Props.DamageHP;
					}
					else
					{
						ShieldHP -= (int)dinfo.Amount * Props.DamageHP * 3;
					}
				}
				if (ShieldHP <= 0)
				{
					Break();
				}
				else
				{
					AbsorbedDamage(dinfo);
				}
				absorbed = true;
			}
		}

		private void AbsorbedDamage(DamageInfo dinfo)
		{
			SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(PawnOwner.Position, PawnOwner.Map));
			Vector3 impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
			Vector3 loc = PawnOwner.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
			float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
			FleckMaker.Static(loc, PawnOwner.Map, FleckDefOf.ExplosionFlash, num);
			int num2 = (int)num;
			for (int i = 0; i < num2; i++)
			{
				FleckMaker.ThrowDustPuff(loc, PawnOwner.Map, Rand.Range(0.8f, 1.2f));
			}
		}

		private void Break()
		{
			EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, 1.0f);
			FleckMaker.Static(PawnOwner.TrueCenter(), PawnOwner.Map, FleckDefOf.ExplosionFlash, 12f);
			for (int i = 0; i < 6; i++)
			{
				FleckMaker.ThrowDustPuff(PawnOwner.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), PawnOwner.Map, Rand.Range(0.8f, 1.2f));
			}
			ShieldHP = 0;
			ShieldActive = false;
			if (Props.AddHediff != null)
			{
				if (PawnOwner.health.hediffSet.HasHediff(Props.AddHediff))
				{
					PawnOwner.health.RemoveHediff(PawnOwner.health.hediffSet.GetFirstHediffOfDef(Props.AddHediff));
				}
			}
		}

		public void Reset()
		{
			if (PawnOwner.Spawned)
			{
				SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(PawnOwner.Position, PawnOwner.Map));
				FleckMaker.ThrowLightningGlow(PawnOwner.TrueCenter(), PawnOwner.Map, 3f);
			}
			ShieldActive = true;
			ShieldHP = Props.HPMax;
			if (Props.AddHediff != null)
			{
				PawnOwner.health.AddHediff(Props.AddHediff);
			}
		}

		public override void CompDrawWornExtras()
		{
			base.CompDrawWornExtras();
			Draw();
		}

		private void Draw()
		{
			if (ShieldState == ShieldState.Active && ShouldDisplay)
			{
				float num = Mathf.Lerp(0.5f, 2.0f, ShieldHP);
				Vector3 drawPos = PawnOwner.Drawer.DrawPos;
				drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
				float angle = Rand.Range(0, 360);
				Vector3 s = new Vector3(num, 1f, num);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, Material_Resur.Shield, 0);
			}
		}

	}

	public class Verb_ActiveShield : Verb
	{
		protected override bool TryCastShot()
		{
			ActiveShield(base.ReloadableCompSource);
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = false;
			return base.EquipmentSource.GetStatValue(StatDefOf.SmokepopBeltRadius);
		}

		public override void DrawHighlight(LocalTargetInfo target)
		{
			DrawHighlightFieldRadiusAroundTarget(caster);
		}

		public static void ActiveShield(CompReloadable comp)
		{
			if (comp != null && comp.CanBeUsed)
			{
				ThingWithComps parent = comp.parent;
				parent.TryGetComp<CompShield>().Reset();
				comp.UsedOnce();
			}
		}
	}

}
