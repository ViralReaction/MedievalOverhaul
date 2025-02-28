﻿using HarmonyLib;
using Verse;

namespace MedievalOverhaul.Patches
{

    [HarmonyPatch(typeof(Book), "GenerateBook")]
    public static class Book_GenerateBook_Patch
    {
        public static void Prefix(ref Pawn author, ref long? fixedDate)
        {
            if (GenRecipe_MakeRecipeProducts.curWorker != null)
            {
                author = GenRecipe_MakeRecipeProducts.curWorker;
                fixedDate = GenTicks.TicksAbs;
            }
        }
    }
}
