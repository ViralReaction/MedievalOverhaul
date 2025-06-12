// using System;
// using RimWorld.Planet;
// using Verse;
// using RimWorld;
// using static UnityEngine.GraphicsBuffer;
//
// namespace MedievalOverhaul
// {
//
//     public class QuestPart_AlpDarkness : QuestPart
//     {
//         public QuestPart_AlpDarkness()
//         {
//         }
//
//         public QuestPart_AlpDarkness(string inSignal, MapParent mapParent)
//         {
//             this.inSignal = inSignal;
//             this.mapParent = mapParent;
//         }
//         public override void Notify_QuestSignalReceived(Signal signal)
//         {
//             if (this.mapParent.Map == null)
//             {
//                 return;
//             }
//             if (signal.tag == this.inSignal)
//             {
//                 var mapBrightnessTracker = this.mapParent.Map.gameConditionManager.mapBrightnessTracker;
//                 mapBrightnessTracker.brightness = mapBrightnessTracker.CurBrightness;
//                 mapBrightnessTracker.targetBrightness = 0f;
//                 mapBrightnessTracker.lerp = 0f;
//                 mapBrightnessTracker.lerpSeconds = 5f;
//                 this.mapParent.Map.gameConditionManager.ownerMap?.mapDrawer.WholeMapChanged(MapMeshFlagDefOf.GroundGlow);
//             }
//         }
//
//         public override void Cleanup()
//         {
//             base.Cleanup();
//             if (this.mapParent.Map != null)
//             {
//                 GameCondition_AlpEclipse activeCondition = this.mapParent.Map.GameConditionManager.GetActiveCondition<GameCondition_AlpEclipse>();
//                 if (activeCondition != null)
//                 {
//                     activeCondition.Permanent = false;
//                     activeCondition.TicksLeft = 301;
//                 }
//                 var mapBrightnessTracker = this.mapParent.Map.gameConditionManager.mapBrightnessTracker;
//                 mapBrightnessTracker.brightness = mapBrightnessTracker.CurBrightness;
//                 mapBrightnessTracker.targetBrightness = 1f;
//                 mapBrightnessTracker.lerp = 0f;
//                 mapBrightnessTracker.lerpSeconds = 5f;
//                 this.mapParent.Map.gameConditionManager.ownerMap?.mapDrawer.WholeMapChanged(MapMeshFlagDefOf.GroundGlow);
//             }
//         }
//
//         public override void ExposeData()
//         {
//             base.ExposeData();
//             Scribe_Values.Look<string>(ref this.inSignal, "inSignal", null, false);
//             Scribe_References.Look<MapParent>(ref this.mapParent, "mapParent", false);
//         }
//
//         public string inSignal;
//
//         public MapParent mapParent;
//     }
// }
