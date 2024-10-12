using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace MedievalOverhaul
{
    public class GameCondition_AlpEclipse : GameCondition_ForceWeather
    {
        public override int TransitionTicks
        {
            get
            {
                return 3000;
            }
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, (float)this.TransitionTicks, 1f);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget?(new SkyTarget(0f, EclipseSkyColors, 1f, 0f));
        }
        public override WeatherDef ForcedWeather()
        {
            Map map = base.AffectedMaps.FirstOrDefault<Map>();
            if (map == null)
            {
                return null;
            }
            return MedievalOverhaulDefOf.DankPyon_AlpEclipse_Weather;
        }
        public override void End()
        {
            base.End();
            foreach (Map map in base.AffectedMaps)
            {
                map.weatherDecider.StartNextWeather();
            }
        }
        public override bool AllowEnjoyableOutsideNow(Map map)
        {
            return base.HiddenByOtherCondition(map);
        }
        public override void GameConditionTick()
        {
            base.GameConditionTick();
            List<Map> affectedMaps = base.AffectedMaps;
            for (int j = 0; j < this.overlays.Count; j++)
            {
                for (int k = 0; k < affectedMaps.Count; k++)
                {
                    this.overlays[j].TickOverlay(affectedMaps[k]);
                }
            }
        }
        public override void GameConditionDraw(Map map)
        {
            if (map.GameConditionManager.MapBrightness > 0.5f)
            {
                return;
            }
            for (int i = 0; i < this.overlays.Count; i++)
            {
                this.overlays[i].DrawOverlay(map);
            }
        }
        private List<SkyOverlay> overlays = new List<SkyOverlay>
        {
            new WeatherOverlay_Fog()
        };

        public static readonly SkyColorSet EclipseSkyColors = new SkyColorSet(new Color(0.17f, 0.17f, 0.17f), Color.white, new Color(0.6f, 0.6f, 0.6f), 1f);
    }
}
