using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public static class CachingUtility
    {
        public static Dictionary<ThingDef, float> FuelValueDict = [];

        static CachingUtility()
        {

            CacheFuelValue();


        }

        public static void CacheFuelValue()
        {
            foreach (ThingDef fuelThing in DefDatabase<ThingDef>.AllDefs)
            {
                var modExtension = fuelThing?.GetModExtension<FuelValueProperty>();
                if (modExtension != null)
                {
                    FuelValueDict.Add(fuelThing, modExtension.fuelValue);
                }
            }

        }

    }
}
