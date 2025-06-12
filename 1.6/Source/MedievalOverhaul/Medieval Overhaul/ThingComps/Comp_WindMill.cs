using RimWorld;
using Verse;
using UnityEngine;
using System;
using Verse.Sound;
using System.Collections.Generic;
using System.Text;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
	public class Comp_WindMill : ThingComp
	{
        private float cachedPowerOutput;
        private float PowerPercent => cachedPowerOutput / 250f;

        public int updateWeatherEveryXTicks = 250;

        private int ticksSinceWeatherUpdate;

        private float spinPosition;

        private Sustainer sustainer;

        private static float SpinRateFactor = 0.035f;

        private static readonly Material WindTurbineBladesMat = MaterialPool.MatFrom("Buildings/Windmill/WindmillBlades");

        private List<IntVec3> windPathCells = new List<IntVec3>();
        private List<Thing> windPathBlockedByThings = new List<Thing>();
        private List<IntVec3> windPathBlockedCells = new List<IntVec3>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			spinPosition = Rand.Range(0f, 15f);
		}

		public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
		{
			base.PostDeSpawn(map);
			if (sustainer != null && !sustainer.Ended)
			{
				sustainer.End();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksSinceWeatherUpdate, "updateCounter", 0);
		}

		public override void CompTick()
		{
			base.CompTick();
			ticksSinceWeatherUpdate++;
			if (ticksSinceWeatherUpdate >= updateWeatherEveryXTicks)
			{
                float windSpeed = Mathf.Min(parent.Map.windManager.WindSpeed, 1.5f);
				ticksSinceWeatherUpdate = 0;
				cachedPowerOutput = 250 * windSpeed;
                this.RecalculateBlockages();
                if (this.windPathBlockedCells.Count > 0)
                {
                    cachedPowerOutput *= BlockageFactor; // Apply blockage effect
                }
                if (cachedPowerOutput < 0f)
                {
                    cachedPowerOutput = 0f;
                }

            }
			if (cachedPowerOutput > 0.01f)
			{
				spinPosition += PowerPercent * SpinRateFactor;
			}
			if (sustainer == null || sustainer.Ended)
			{
				sustainer = SoundDefOf.WindTurbine_Ambience.TrySpawnSustainer(SoundInfo.InMap(parent));
			}
			sustainer.Maintain();
		}

		public override void PostDraw()
		{
			base.PostDraw();
			Vector3 vector = parent.TrueCenter();
			vector += parent.Rotation.FacingCell.ToVector3() * 2.36f;
			for (int i = 0; i < 9; i++)
			{
				float num = spinPosition + (float)Math.PI * 2f * (float)i / 9f;
				float x = Mathf.Abs(4f * Mathf.Sin(num));
				bool num2 = num % ((float)Math.PI * 2f) < (float)Math.PI;
				Vector2 vector2 = new Vector2(x, 1f);
				Vector3 s = new Vector3(vector2.x, 1f, vector2.y);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(vector + Vector3.up * (3f / 74f) * Mathf.Cos(num), parent.Rotation.AsQuat, s);
				Graphics.DrawMesh(num2 ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, WindTurbineBladesMat, 0);
			}
		}

        // Calculates how much the wind blockage affects output (1.0 = no blockage, 0.0 = fully blocked)
        public float BlockageFactor
        {
            get
            {
                if (this.windPathBlockedCells.Count == 0) return 1f; // No blockages, full efficiency

                float blockedEffect = 1f - (this.windPathBlockedCells.Count * 0.2f);
                return Mathf.Clamp(blockedEffect, 0f, 1f); // Keeps within 0% - 100%
            }
        }

        private void RecalculateBlockages()
        {
            if (this.windPathCells.Count == 0)
            {
                IEnumerable<IntVec3> collection = WindMillUtility.CalculateWindCells(this.parent.Position, this.parent.Rotation, this.parent.def.size);
                this.windPathCells.AddRange(collection);
            }
            this.windPathBlockedCells.Clear();
            this.windPathBlockedByThings.Clear();
            for (int i = 0; i < this.windPathCells.Count; i++)
            {
                IntVec3 intVec = this.windPathCells[i];
                if (this.parent.Map.roofGrid.Roofed(intVec))
                {
                    this.windPathBlockedByThings.Add(null);
                    this.windPathBlockedCells.Add(intVec);
                }
                else
                {
                    List<Thing> list = this.parent.Map.thingGrid.ThingsListAt(intVec);
                    for (int j = 0; j < list.Count; j++)
                    {
                        Thing thing = list[j];
                        if (thing.def.blockWind)
                        {
                            this.windPathBlockedByThings.Add(thing);
                            this.windPathBlockedCells.Add(intVec);
                            break;
                        }
                    }
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.windPathBlockedCells.Count > 0)
            {
                Thing thing = null;
                if (this.windPathBlockedByThings != null)
                {
                    thing = this.windPathBlockedByThings[0];
                }
                if (thing != null)
                {
                    stringBuilder.Append("WindTurbine_WindPathIsBlockedBy".Translate() + " " + thing.Label);
                }
                else
                {
                    stringBuilder.Append("WindTurbine_WindPathIsBlockedByRoof".Translate());
                }
                return stringBuilder.ToString();
            }
            return null;
            
        }

    }
}
