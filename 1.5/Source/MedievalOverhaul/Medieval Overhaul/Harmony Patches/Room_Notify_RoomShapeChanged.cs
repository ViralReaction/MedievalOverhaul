using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(Room), nameof(Room.Notify_RoomShapeChanged))]
    public static class Room_Notify_RoomShapeChanged
    {

        public static void Postfix(ref Room __instance)
        {
            if (!__instance.Dereferenced)
            {
                var hashSet = __instance.Map.GetComponent<RefuelableMapComponent>().refuelableStatThing;
                foreach (var thing in hashSet)
                {
                    thing.Notify_ColorChanged();
                }
            }
        }
    }
}
