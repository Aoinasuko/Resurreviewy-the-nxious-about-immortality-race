using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Resurreviewy_Race
{

    [StaticConstructorOnStartup]
    static class Resurreviewy_Harmony
    {
        static Resurreviewy_Harmony()
        {
            var harmony = new Harmony("BEP.Resur");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    // ムード制限(リザレビューイの場合50%に）
    [HarmonyPatch(typeof(Need_Mood), "CurInstantLevel")]
    static class FixMood_Ins
    {
        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Getter)]
        static bool Prefix(ref float __result, ref Need_Mood __instance)
        {
            if (__instance.thoughts.pawn.def.defName == "Resurreviewy_Pawn")
            {
                if (!Util.isundead(__instance.thoughts.pawn))
                {
                    return true;
                }
                float num = __instance.thoughts.TotalMoodOffset();
                if (__instance.thoughts.pawn.IsColonist || __instance.thoughts.pawn.IsPrisonerOfColony)
                {
                    num += Find.Storyteller.difficulty.colonistMoodOffset;
                }
                __result = Math.Min(Mathf.Clamp01((__instance.def.baseLevel / 2f) + num / 100f), 0.50f);
                return false;
            }
            return true;
        }
    }

    // ムード制限の説明
    [HarmonyPatch(typeof(Need_Mood), "GetTipString")]
    static class FixMood_GetTipString
    {
        [HarmonyPostfix]
        static void Postfix(ref string __result, ref Need_Mood __instance)
        {
            if (__instance.thoughts.pawn.def.defName == "Resurreviewy_Pawn")
            {
                if (!Util.isundead(__instance.thoughts.pawn))
                {
                    return;
                }
                __result = __result + "\n" + "Resur.UI.MaxMoodLimitTip".Translate().Colorize(Color.red);
                return;
            }
            return;
        }
    }

    /// <summary>
    /// リザレビィ達は（基本）死なない
    /// </summary>    
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDead")]
    static class FixDeath
    {
        [HarmonyPrefix]
        static bool Prefix(ref Pawn_HealthTracker __instance, ref bool __result)
        {
            Pawn pawn = Traverse.Create(root: __instance).Field(name: "pawn").GetValue<Pawn>();
            if (!Util.isundead(pawn))
            {
                return true;
            }
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    [HarmonyPatch(new Type[]
    {
        typeof(DamageInfo?),
        typeof(Hediff)
    })]
    static class FixKill
    {
        [HarmonyPrefix]
        static bool Prefix(ref Pawn __instance)
        {
            if (!Util.isundead(__instance))
            {
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// リザレビィ達は（基本）老いない
    /// </summary>
    [HarmonyPatch(typeof(Pawn_AgeTracker), "AgeTick")]
    static class FixAgeTick
    {
        [HarmonyPrefix]
        static bool Prefix(Pawn_AgeTracker __instance)
        {
            Pawn pawn = Traverse.Create(root: __instance).Field(name: "pawn").GetValue<Pawn>();
            if (Util.isundead(pawn))
            {
                if (pawn.health.hediffSet.HasHediff(Hediff_Resur.Resur_EternalLife))
                {
                    return false;
                }
                if (pawn.ageTracker.AgeBiologicalYears >= 18.0f)
                {
                    return false;
                }
            }
            return true;
        }
    }

    // テイム出来るようにする
    [HarmonyPatch(typeof(TameUtility), "CanTame")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
    })]
    internal class Fix_CanTame
    {
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result, Pawn pawn)
        {
            if (pawn.def.defName == "Resurreviewy_Pawn" || pawn.def.defName == "Resurrevi")
            {
                Faction resurreviewy_faction = FactionUtility.DefaultFactionFrom(Faction_Resur.Resur_WildResurreviewy);
                if (resurreviewy_faction != null)
                {
                    if (pawn.Faction != null)
                    {
                        if (pawn.Faction == resurreviewy_faction)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
    }

    // リザレビューイ達は逃げ出さない
    [HarmonyPatch(typeof(MentalBreakWorker_RunWild), "BreakCanOccur")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
    })]
    internal class Fix_RunWildBreakCanOccur
    {
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result, Pawn pawn)
        {
            if (pawn.def.defName == "Resurreviewy_Pawn")
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

}
