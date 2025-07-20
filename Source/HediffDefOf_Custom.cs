using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.TrueVacuum
{
    [DefOf]
    public static class DefOf_Custom
    {
        public static BodyPartDef Nose;
        public static BodyPartDef Stomach;
        public static BodyPartDef Tongue;

        public static HediffDef VacuumBurn;

        public static DamageDef TV_SoftTissueVacuumBurn;
        public static HediffDef TV_SoftTissueVacuumBleed;
        public static HediffDef TV_DecompressionSickness;

        static DefOf_Custom()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOf_Custom));
        }
    }
}
