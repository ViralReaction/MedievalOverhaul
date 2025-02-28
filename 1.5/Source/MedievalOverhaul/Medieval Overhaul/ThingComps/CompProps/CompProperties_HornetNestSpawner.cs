﻿using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class CompProperties_HornetNestSpawner : CompProperties

    {
        public List<string> spawnablePawnKinds;
        public SoundDef spawnSound;
        public float defendRadius = 20f;
        public int initialPawnCount;
        public int maxPawnCount = 20;
        public FloatRange pawnSpawnIntervalDays = new FloatRange(0.5f, 2f);
        public int pawnSpawnRadius = 2;
        public string nextSpawnInspectStringKey;
        public bool spawnAsPlayerFaction = false;
        public FactionDef faction;

        public CompProperties_HornetNestSpawner() => this.compClass = typeof(Comp_HornetNestSpawner);

    }
}