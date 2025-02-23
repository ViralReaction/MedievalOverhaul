using Verse;

namespace MedievalOverhaul
{
    public static class OiledWeaponsManager
    {
        public static OiledWeaponsComponent GetComp()
        {
            return Current.Game.GetComponent<OiledWeaponsComponent>();
        }
    }
}
