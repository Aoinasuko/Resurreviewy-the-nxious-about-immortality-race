using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace Resurreviewy_Race
{
	class IncidentWorker_ResurreviewyPass : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
			{
				return false;
			}
			if (!map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(ThingDef.Named("Resurreviewy_Pawn")))
			{
				return false;
			}
			IntVec3 cell;
			return TryFindEntryCell(map, out cell);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryFindEntryCell(map, out IntVec3 cell))
			{
				return false;
			}
			if (!TryFindFormerFaction(out Faction formerFaction))
			{
				return false;
			}
			List<Pawn> pawns = new List<Pawn>();
			PawnKindDef Resurreviewy = PawnKindDef.Named("Resurreviewy_Visitor");
			int max = Rand.RangeInclusive(2, 4);
			Pawn pawn = null;
			for (int i = 0; i < max; i++)
			{
				pawn = PawnGenerator.GeneratePawn(Resurreviewy, Find.FactionManager.FirstFactionOfDef(Faction_Resur.Resur_WildResurreviewy));
				pawn.kindDef = PawnKindDefOf.WildMan;
				GenSpawn.Spawn(pawn, cell, map);
				pawns.Add(pawn);
			}
			for (int i = 0; i < 2; i++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(cell, map, 10);
				pawn = PawnGenerator.GeneratePawn(PawnKindDef.Named("Resurrevi"));
				pawn.SetFaction(Find.FactionManager.FirstFactionOfDef(Faction_Resur.Resur_WildResurreviewy));				
				GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
				pawns.Add(pawn);
			}
			RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out IntVec3 result);
			LordMaker.MakeNewLord(Find.FactionManager.FirstFactionOfDef(Faction_Resur.Resur_WildResurreviewy), new LordJob_VisitColony(Find.FactionManager.FirstFactionOfDef(Faction_Resur.Resur_WildResurreviewy), result, 60000), map, pawns);
			SendStandardLetter("Resur.Incident.LetterLabelResurreviPass".Translate().CapitalizeFirst(), "Resur.Incident.LetterResurreviPass".Translate(), LetterDefOf.NeutralEvent, parms, pawn);
			return true;
		}

		private bool TryFindEntryCell(Map map, out IntVec3 cell)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore, out cell);
		}

		private bool TryFindFormerFaction(out Faction formerFaction)
		{
			return Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out formerFaction, tryMedievalOrBetter: false, allowDefeated: true);
		}
	}
}
