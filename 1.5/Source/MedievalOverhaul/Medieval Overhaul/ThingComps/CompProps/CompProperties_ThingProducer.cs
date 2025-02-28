﻿using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class CompProperties_ThingProducer : CompProperties
    {
        public List<Item> items;
        public bool random = true;
        public int timeRequired = 300000;
        public bool displayMessage = true;
        public bool requireFuel = false;
        public bool requireFuelCustom = false;
        public bool requirePower = false;
        public bool onlyUnroofed = false;
        public bool restAtNight = false;
        public bool inTempRange = false;
        public FloatRange viableTempRange = new FloatRange(10f, 35f);
        public List<WeatherDef> disabledWeathers;
        public bool invertWeathers = false;

        public CompProperties_ThingProducer() => this.compClass = typeof(Comp_ThingProducer);
    }
}
