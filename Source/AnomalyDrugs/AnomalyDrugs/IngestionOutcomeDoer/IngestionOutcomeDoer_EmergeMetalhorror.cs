using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomalyDrugs
{
    public class IngestionOutcomeDoer_EmergeMetalhorror : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (MetalhorrorUtility.IsInfected(pawn))
            {
                MetalhorrorUtility.TryEmerge(pawn);
                DeletePathways(pawn.infectionVectors);
            }
            else
            {
                if (pawn.infectionVectors.AnyPathwayForHediff(HediffDefOf.MetalhorrorImplant))
                    Find.LetterStack.ReceiveLetter("CatalystolPathwayLabel".Translate(), "CatalystolPathwayText".Translate(pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
                else
                    Find.LetterStack.ReceiveLetter("CatalystolOccursNothingLabel".Translate(), "CatalystolOccursNothingText".Translate(pawn.Named("PAWN")), LetterDefOf.NeutralEvent);
            }
        }

        private void DeletePathways(Pawn_InfectionVectorTracker tracker)
        {
            var dict = (Dictionary<InfectionPathwayDef, InfectionPathway>) AccessTools.Field(typeof(Pawn_InfectionVectorTracker), "pathways").GetValue(tracker);
            foreach (var pathway in HediffDefOf.MetalhorrorImplant.possiblePathways)
                dict.Remove(pathway.PathwayDef);
        }
    }
}