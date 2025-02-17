﻿using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class RecipeWorkerCounter_GrindWheat : RecipeWorkerCounter
    {
        public override bool CanCountProducts(Bill_Production bill)
        {
            return this.recipe.specialProducts == null && this.recipe.products != null && this.recipe.products.Count >= 1;
        }

        public override string ProductsDescription(Bill_Production bill)
        {
            return MedievalOverhaulDefOf.DankPyon_Flour.label;
        }

    }
}
