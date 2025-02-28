using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]

    // Using this to set changes to specific Defs
    public class Setting_Utility
    {
        static Setting_Utility() 
        {

            UpdateSettings();


        }

        public static void UpdateSettings()
        {
            UpdateSoilFertility();
        }

        public static void UpdateSoilFertility()
        {
            TerrainDef plowedSoil = DefDatabase<TerrainDef>.GetNamed("DankPyon_PlowedSoil", false);
            if (plowedSoil != null)
            {
                plowedSoil.fertility = MedievalOverhaulSettings.settings.plowedFertility;
            }
        }
    }
}
