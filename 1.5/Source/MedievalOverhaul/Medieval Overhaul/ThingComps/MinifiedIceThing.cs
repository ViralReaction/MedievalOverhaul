using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using System.Text;
using Mono.Unix.Native;
using System.Collections.Generic;

namespace MedievalOverhaul
{
    public class MinifiedIceThing : MinifiedThing
    {
        public Thing innerThing => this.InnerThing;
        public CompMeltable compMeltable
        {
            get
            {
                var comp = innerThing.TryGetComp<CompMeltable>();
                if (comp is null)
                {
                    Log.Error($"Medieval Overhaul: [CompMeltable] Missing CompMeltable on {innerThing?.def?.defName ?? "UNKNOWN"} (ThingID: {innerThing?.ThingID ?? "NULL"})");
                    return null;
                }
                return comp;
            }
        }
        public bool Active
        {
            get
            {
                if (this.Map != null)
                {
                    return !this.Position.GetThingList(this.Map).Any((Thing x) => x.def == MedievalOverhaulDefOf.DankPyon_IceCellar);
                }
                return false;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (this.cachedGraphic == null)
                {
                    this.cachedGraphic = base.InnerThing.Graphic.ExtractInnerGraphicFor(base.InnerThing, null);
                    Vector2 minifiedDrawSize = base.GetMinifiedDrawSize(base.InnerThing.def.size.ToVector2(), 1.1f);
                    Vector2 newDrawSize = new Vector2(minifiedDrawSize.x / (float)base.InnerThing.def.size.x * this.cachedGraphic.drawSize.x, minifiedDrawSize.y / (float)base.InnerThing.def.size.z * this.cachedGraphic.drawSize.y);
                    this.cachedGraphic = this.cachedGraphic.GetCopy(newDrawSize, ShaderTypeDefOf.Cutout.Shader);
                }
                return this.cachedGraphic;
            }
        }

        public override Graphic LoadCrateFrontGraphic()
        {
            return GraphicDatabase.Get<Graphic_Single>("Things/Item/Minified/BurlapBag", ShaderDatabase.Cutout, base.GetMinifiedDrawSize(base.InnerThing.def.size.ToVector2(), 1.1f) * 1.16f, Color.white);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool spawned = base.Spawned;
            Map map = base.Map;
            base.Destroy(mode);
            if (innerThing != null)
            {
                InstallBlueprintUtility.CancelBlueprintsFor(this);
                if (spawned)
                {
                    if (mode == DestroyMode.Deconstruct)
                    {
                        SoundDefOf.Building_Deconstructed.PlayOneShot(new TargetInfo(base.Position, map, false));
                        GenLeaving.DoLeavingsFor(this.InnerThing, map, mode, this.OccupiedRect(), null, null);
                    }
                    else if (mode == DestroyMode.KillFinalize)
                    {
                        GenLeaving.DoLeavingsFor(this.InnerThing, map, mode, this.OccupiedRect(), null, null);
                    }
                }
                if (this.InnerThing is MonumentMarker)
                {
                    this.InnerThing.Destroy(DestroyMode.Vanish);
                }
            }
        }

        public override void PostMake()
        {
            base.PostMake();
        }
        public override void Tick()
        {
            base.Tick();
            this.Tick(1);
        }

        public override void TickRare()
        {
            base.TickRare();
            this.Tick(250);
        }

        private void Tick(int interval)
        {
            if (!Active)
            {
                return;
            }
            float num = MeltRateAtTemperature(this.AmbientTemperature);
            compMeltable.MeltProgress += num * interval;
            if (compMeltable.Stage == RotStage.Rotting)
            {
                if (innerThing.IsInAnyStorage() && innerThing.SpawnedOrAnyParentSpawned)
                {
                    Messages.Message("MessageRottedAwayInStorage".Translate(innerThing.Label, innerThing).CapitalizeFirst(), new TargetInfo(innerThing.PositionHeld, innerThing.MapHeld, false), MessageTypeDefOf.NegativeEvent, true);
                    LessonAutoActivator.TeachOpportunity(ConceptDefOf.SpoilageAndFreezers, OpportunityType.GoodToKnow);
                }
                this.Destroy(DestroyMode.Vanish);
                //innerThing.Destroy(DestroyMode.Vanish);
                return;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if ((float)compMeltable.PropsRot.TicksToMeltStart - compMeltable.MeltProgress > 0f)
            {
                float num = MeltRateAtTemperature((float)Mathf.RoundToInt(innerThing.AmbientTemperature));
                int ticksUntilMeltAtCurrentTemp = TicksUntilMeltAtCurrentTemp;
                float percentage = compMeltable.MeltProgressPct * 100;
                stringBuilder.Append(percentage.ToString("F2") + "% " + "Melted");
                if (num < 0.001f)
                {
                    stringBuilder.Append(string.Format(" ({0})", "CurrentlyFrozen".Translate()));
                }
                else
                    stringBuilder.AppendTagged(string.Format(" ({0})", "DankPyon_Melted".Translate() + " " + ticksUntilMeltAtCurrentTemp.ToStringTicksToPeriod(true, false, true, true, false)));
            }
            return stringBuilder.ToString();
        }

        public static float MeltRateAtTemperature(float temperature)
        {
            if (temperature < 0f)
            {
                return 0f;
            }
            if (temperature >= 20f)
            {
                return 1f;
            }
            return (temperature - 0f) / 20f;
        }
        public int TicksUntilMeltAtTemp(float temp)
        {
            if (!this.Active)
            {
                return 72000000;
            }
            float num = MeltRateAtTemperature(temp);
            if (num <= 0f)
            {
                return 72000000;
            }
            float num2 = (float)compMeltable.PropsRot.TicksToMeltStart - compMeltable.MeltProgress;
            if (num2 <= 0f)
            {
                return 0;
            }
            return Mathf.RoundToInt(num2 / num);
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Fully Melt",
                    action = delegate ()
                    {
                        compMeltable.MeltProgress += compMeltable.MeltProgress;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Melt 10%",
                    action = delegate ()
                    {
                        compMeltable.MeltProgress += compMeltable.PropsRot.TicksToMeltStart * 0.10f;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Melt 50%",
                    action = delegate ()
                    {
                        compMeltable.MeltProgress += compMeltable.PropsRot.TicksToMeltStart * 0.50f;
                    }
                };
            }
            yield break;
        }
        public int TicksUntilMeltAtCurrentTemp
        {
            get
            {
                float ambientTemperature = this.AmbientTemperature;
                ambientTemperature = Mathf.RoundToInt(ambientTemperature);
                return TicksUntilMeltAtTemp(ambientTemperature);
            }
        }
    }
}
