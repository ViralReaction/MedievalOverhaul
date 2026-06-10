

using UnityEngine;
using Verse;
namespace TransparentThings
{
    public class TransparentThingsSettings : ModSettings
    {
        public bool enableTreeTransparency = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.enableTreeTransparency, "enableTreeTransparency", false);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("TT.EnableTreeTransparency".Translate(), ref this.enableTreeTransparency);
            listingStandard.End();
        }
    }
}
