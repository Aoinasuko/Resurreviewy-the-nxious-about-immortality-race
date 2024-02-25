using LittleFairy_Race;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Resurreviewy_Race
{
    public class Projectile_SoulBullet : Projectile
    {
        private int ticksToDetonation;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation", 0);
        }

        public override void Tick()
        {
            base.Tick();
            if (ticksToDetonation > 0)
            {
                ticksToDetonation--;
                if (ticksToDetonation <= 0)
                {
                    Explode();
                }
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (blockedByShield || def.projectile.explosionDelay == 0)
            {
                Explode();
                return;
            }
            landed = true;
            ticksToDetonation = def.projectile.explosionDelay;
        }

        protected void Explode()
        {
            Map map = base.Map;
            Pawn target = null;
            // レーザーを生み出し近い敵に攻撃
            List <Pawn> pawns = this.Map.mapPawns.AllPawnsSpawned.Where(x => x.Position.DistanceTo(this.Position) <= 10.9f && x.HostileTo(this.Launcher)).ToList();
            if (!pawns.Where(x => !x.Downed).EnumerableNullOrEmpty())
            {
                target = pawns.Where(x => !x.Downed).RandomElement();
            } else
            {
                target = pawns.RandomElement();
            }
            if (target != null)
            {
                Shot(target);
            }
            Destroy();
        }

        private void Shot(Pawn pawn)
        {
            Projectile shot = (Projectile)GenSpawn.Spawn(ThingDef.Named("Resur_SoulLaser"), pawn.Position, pawn.Map);
            Vector3 drawPos = this.DrawPos;
            IntVec3 c = pawn.Position;
            shot.Launch(this.Launcher, drawPos, c, pawn, ProjectileHitFlags.All, false, null);
        }

    }

    public class Projectile_SoulLaser : Projectile
    {
        public override void Tick()
        {
            int i = 0;
            while (true)
            {
                i++;
                if (landed)
                {
                    break;
                }
                Vector3 exactPosition = ExactPosition;
                ticksToImpact--;
                if (!ExactPosition.InBounds(base.Map))
                {
                    ticksToImpact++;
                    base.Position = ExactPosition.ToIntVec3();
                    DrawLaser(DrawPos);
                    Destroy();
                    break;
                }
                Vector3 exactPosition2 = ExactPosition;
                base.Position = ExactPosition.ToIntVec3();
                if (i > 2)
                {
                    DrawLaser(DrawPos);
                }
                if (ticksToImpact <= 0)
                {
                    if (DestinationCell.InBounds(base.Map))
                    {
                        base.Position = DestinationCell;
                    }
                    SoundDef.Named("InfernoCannon_Fire").PlayOneShot(new TargetInfo(this.Position, this.Map, false));
                    if (this.intendedTarget.Pawn != null)
                    {
                        Pawn pawn = this.intendedTarget.Pawn;
                        pawn.TakeDamage(new DamageInfo(DamageDefOf.Bomb, 1.0f, 1.0f, -1, this, null, null));
                        if (!pawn.Dead)
                        {
                            // 耐性貫通ダメージ付与
                            BodyPartRecord bodyPartRecord = (from p in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside)
                                                             where p.def.defName.ToStringSafe() != "Waist"
                                                             select p).RandomElement();
                            float severity = this.DamageAmount;
                            Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Burn, pawn, bodyPartRecord);
                            hediff.Severity = severity;
                            pawn.health.AddHediff(hediff, bodyPartRecord);
                        }
                    }                    
                    DrawLaser(DrawPos);
                    Destroy();
                    break;
                }
            }
            return;
        }

        public void DrawLaser(Vector3 pos)
        {
            MoteMaker.MakeStaticMote(pos, this.Map, ThingDef.Named("Resur_Mote_Soul"), 0.5f);
        }

        public override void Draw()
        {
            Comps_PostDraw();
        }

    }

}
