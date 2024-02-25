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

    public static class Resur_ExMethod
    {
        // 拡張メゾット
        public static void AddResearchDirect(this ResearchManager manage)
        {
            FieldInfo field = manage.GetType().GetField("progress", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<ResearchProjectDef, float> projects = (Dictionary<ResearchProjectDef, float>)field.GetValue(manage);

            ResearchProjectDef cproj = manage.currentProj;

            float num = manage.GetProgress(cproj);
            int addval = 1;
            if (cproj.techLevel == TechLevel.Industrial)
            {
                addval = 2;
            }
            if (cproj.techLevel <= TechLevel.Medieval)
            {
                addval = 4;
            }
            num += addval;
            projects[cproj] = Math.Min(cproj.CostApparent - 1.0f, num);
        }
    }

}
