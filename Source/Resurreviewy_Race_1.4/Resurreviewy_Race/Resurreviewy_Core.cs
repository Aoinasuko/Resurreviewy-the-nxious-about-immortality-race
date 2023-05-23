using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Resurreviewy_Race
{
    [DefOf]
    public static class Thought_Resur
    {
        public static ThoughtDef Resur_PsychologicalEffect;
    }

    [DefOf]
    public static class Hediff_Resur
    {
        public static HediffDef Resur_EternalLife;
        public static HediffDef Resur_Antidote_EternalLife;
    }

    [DefOf]
    public static class Job_Resur
    {
        public static JobDef Resur_UseItem;
        public static JobDef Resur_Buried;
    }

    [DefOf]
    public static class FleshType_Resur
    {
        public static FleshTypeDef Resurreviewy;
    }

    [DefOf]
    public static class Faction_Resur
    {
        public static FactionDef Resur_WildResurreviewy;
    }

    [StaticConstructorOnStartup]
    public static class Material_Resur
    {
        public static readonly Material Shield = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
    }
}
