using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    class RecipeExtension_Mine : DefModExtension
    {
        #pragma warning disable CS0649
        public List<ThingDef> bonusGems;
        #pragma warning restore CS0649
        public float randomChance = 0.01f;
        public float workAmountPerChance = 600f;
        public static RecipeExtension_Mine Get(Def def)
        {
            return def.GetModExtension<RecipeExtension_Mine>();
        }
    }
}
