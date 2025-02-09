using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class OiledWeaponsComponent : GameComponent
    {
        public HashSet<ThingWithComps> oiledWeapons = new();
        private Dictionary<int, CompWeaponOilData> savedCompData = new(); // Stores oil data separately

        public OiledWeaponsComponent(Game game) { }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (var weapon in oiledWeapons)
                {
                    var oilComp = weapon.TryGetComp<CompWeaponOil>();
                    if (oilComp != null)
                    {
                        savedCompData[weapon.thingIDNumber] = new CompWeaponOilData(oilComp);
                    }
                }
            }

            Scribe_Collections.Look(ref oiledWeapons, "oiledWeapons", LookMode.Reference);
            Scribe_Collections.Look(ref savedCompData, "savedCompData", LookMode.Value, LookMode.Deep);

            if (oiledWeapons == null)
                oiledWeapons = new HashSet<ThingWithComps>();

            if (savedCompData == null)
                savedCompData = new Dictionary<int, CompWeaponOilData>();
        }

        public override void LoadedGame()
        {
            oiledWeapons.RemoveWhere(weapon => weapon == null || weapon.Destroyed);

            foreach (var weapon in oiledWeapons)
            {
                CompWeaponOil oilComp = weapon.TryGetComp<CompWeaponOil>();

                if (oilComp == null)
                {
                    oilComp = new CompWeaponOil();
                    weapon.AllComps.Add(oilComp);
                    oilComp.parent = weapon;

                    Log.Message($"[LoadedGame] Manually adding CompWeaponOil to {weapon.LabelCap}");

                    // ✅ Restore previously saved data
                    var savedData = GetSavedCompData(weapon);
                    if (savedData != null)
                    {
                        savedData.ApplyTo(oilComp);
                        Log.Message($"[LoadedGame] Restored saved oil data for {weapon.LabelCap}");
                    }
                    else
                    {
                        Log.Warning($"[LoadedGame] No saved oil data for {weapon.LabelCap}, using defaults.");
                    }
                }
            }
        }

        public void RegisterOilWeapon(ThingWithComps weapon, CompWeaponOil comp)
        {
            if (weapon != null && comp != null)
            {
                oiledWeapons.Add(weapon);
                savedCompData[weapon.thingIDNumber] = new CompWeaponOilData(comp);
            }
        }

        public CompWeaponOilData GetSavedCompData(ThingWithComps weapon)
        {
            if (savedCompData.TryGetValue(weapon.thingIDNumber, out var compData))
            {
                return compData;
            }
            return null;
        }
    }
}