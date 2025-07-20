using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.AccessTools;

namespace PeteTimesSix.TrueVacuum
{
    public class HediffGiver_TrueVacuumBurn : HediffGiver_VacuumBurn
    {
        public static SimpleCurve field_vacuumBurnRate;
        public static IntRange field_burnDamageRange;

        private static readonly SimpleCurve severityBurnRateMultiplier = new SimpleCurve
        {
            new CurvePoint(0.2f, 1f),
            new CurvePoint(0.35f, 0.5f),
            new CurvePoint(0.5f, 0.25f),
            new CurvePoint(0.9f, 0.1f),
            new CurvePoint(1f, 0.04f)
        };

        public static float decompressionSicknessMinSeverity = 0.35f;
        public static float softTissueBurnMinSeverity = 0.35f;
        public static float softTissueBurnChance = 0.15f;

        private static bool lazyInitialized = false;
        private static List<BodyPartDef> _softTissues;

        private static List<BodyPartDef> SoftTissues
        {
            get
            {
                if(!lazyInitialized)
                {
                    _softTissues = new List<BodyPartDef>() {
                        BodyPartDefOf.Eye,
                        BodyPartDefOf.Lung,
                        DefOf_Custom.Nose,
                        DefOf_Custom.Stomach,
                        DefOf_Custom.Tongue
                    };
                }
                return _softTissues;
            }
        }
        private static readonly IntRange softTissueBurnDamageRange = new IntRange(4, 8);


        static HediffGiver_TrueVacuumBurn()
        {
            field_vacuumBurnRate = StaticFieldRefAccess<SimpleCurve>(typeof(HediffGiver_VacuumBurn), "VacuumSecondsBurnRate");
            field_burnDamageRange = StaticFieldRefAccess<IntRange>(typeof(HediffGiver_VacuumBurn), "BurnDamageRange");
        }

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (!ModsConfig.OdysseyActive || !pawn.Spawned || !pawn.Map.Biome.inVacuum || !pawn.HarmedByVacuum)
                return;

            float vacuumFraction = pawn.Position.GetVacuum(pawn.Map);
            if (vacuumFraction < 0.5f)
                return;

            var exposureHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.VacuumExposure);
            if (exposureHediff == null)
                return;

            var exposureSeverity = exposureHediff.Severity;
            ApplyBurns(pawn, vacuumFraction, exposureSeverity);
            ApplyDecompressionSickness(pawn, vacuumFraction, exposureSeverity);
        }

        private void ApplyDecompressionSickness(Pawn pawn, float vacuumFraction, float exposureSeverity)
        {
            if((exposureSeverity * vacuumFraction) < decompressionSicknessMinSeverity) 
                return;

            var decompressionSicknessHediff = pawn.health.GetOrAddHediff(DefOf_Custom.TV_DecompressionSickness);
            var stages = DefOf_Custom.TV_DecompressionSickness.stages;
            var lastStage = stages[stages.Count - 1];
            //reset any preexisting decompression sickness 
            if (decompressionSicknessHediff.Severity < lastStage.minSeverity)
                decompressionSicknessHediff.Severity = lastStage.minSeverity - 0.001f;
        }

        private void ApplyBurns(Pawn pawn, float vacuumFraction, float exposureSeverity)
        {
            if (exposureSeverity < severityBurnRateMultiplier.Points[0].x)
                return;

            var premultSecondsBetweenBurns = field_vacuumBurnRate.Evaluate(vacuumFraction);
            var ticksBetweenBurns = (premultSecondsBetweenBurns * severityBurnRateMultiplier.Evaluate(exposureSeverity)).SecondsToTicks();
            int ticksSinceLastBurn = Math.Min(GenTicks.TicksGame - pawn.lastVacuumBurntTick, premultSecondsBetweenBurns.SecondsToTicks());

            //Log.Message($"pawn: {pawn.LabelCap} ticksBetween: {ticksBetweenBurns}, sinceLast: {ticksSinceLastBurn}");

            while (ticksSinceLastBurn >= ticksBetweenBurns)
            {
                pawn.lastVacuumBurntTick = GenTicks.TicksGame;
                ticksSinceLastBurn -= ticksBetweenBurns;

                if (exposureSeverity > softTissueBurnMinSeverity && Rand.Chance(softTissueBurnChance))
                {
                    ApplySoftTissueBurn(pawn, exposureSeverity);
                }
                else
                {
                    if (VacuumUtility.TryGetVacuumBurnablePart(pawn, out var p))
                    {
                        DamageInfo dinfo = new DamageInfo(DamageDefOf.VacuumBurn, field_burnDamageRange.RandomInRange, 999f, -1f, null, p);
                        pawn.TakeDamage(dinfo);
                    }
                }
            }
        }

        private void ApplySoftTissueBurn(Pawn pawn, float vacuumFraction)
        {
            var pickedPart = SoftTissues.RandomElement();
            var matchingParts = pawn.health.hediffSet.GetNotMissingParts().Where((BodyPartRecord p) => p.def == pickedPart);
            if (matchingParts.Any())
            {
                var part = matchingParts.RandomElement();
                if (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == part && x.def.countsAsAddedPartOrImplant))
                {
                    //Log.Message($"picked {part?.Label ?? "NULL"} to burn");
                    DamageInfo dinfo = new DamageInfo(DefOf_Custom.TV_SoftTissueVacuumBurn, softTissueBurnDamageRange.RandomInRange, 999f, -1f, null, part);
                    pawn.TakeDamage(dinfo);
                }
                else {
                    //having an artificial body part saves the pawn from this instance of damage
                    //Log.Message($"picked {part?.Label ?? "NULL"} to burn, but its artificial");
                }
            }
        }
    }
}
