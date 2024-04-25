using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AnomalyDrugs
{
    [StaticConstructorOnStartup]
    public class AnomalyDrugs
    {
        private static readonly Type patchType = typeof(AnomalyDrugs);
        private static Harmony harmony;

        static AnomalyDrugs()
        {
            harmony = new Harmony("ng.lyu.anomalydrugs");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            ThingCategoryDef category = DefDatabase<ThingCategoryDef>.GetNamed("EntitySamples", true);
            category.icon = LoadedModManager.RunningMods.FirstOrDefault().GetContentHolder<Texture2D>().Get("Textures/UI/Icons/EntitySamples.png");
        }
    }
}