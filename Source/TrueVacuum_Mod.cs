using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.TrueVacuum
{
    public class TrueVacuum_Mod : Mod
    {
        public static TrueVacuum_Mod ModSingleton { get; private set; }

        public static Harmony Harmony { get; internal set; }

        public TrueVacuum_Mod(ModContentPack content) : base(content)
        {
            ModSingleton = this;

            Harmony = new Harmony("PeteTimesSix.TrueVacuum");
            Harmony.PatchAll();
        }
    }


    [StaticConstructorOnStartup]
    public static class TrueVacuum_PostInit
    {
        static TrueVacuum_PostInit()
        {
        }
    }

}
