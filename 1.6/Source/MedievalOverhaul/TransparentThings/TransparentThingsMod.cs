using UnityEngine;
using Verse;

namespace TransparentThings
{
    public class TransparentThingsMod : Mod
    {
        public static TransparentThingsSettings settings;

        public TransparentThingsMod(ModContentPack pack)
                : base(pack)
        {
            TransparentThingsMod.settings = this.GetSettings<TransparentThingsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "TT.TransparentThings".Translate();
    }
}
