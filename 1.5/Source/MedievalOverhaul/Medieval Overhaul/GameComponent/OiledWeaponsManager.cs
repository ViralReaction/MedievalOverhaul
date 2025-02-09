using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
