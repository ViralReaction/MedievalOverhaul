using RimWorld;
using UnityEngine;
using Verse;

namespace TransparentThings
{
    public class Graphic_TransparentThing : Graphic_StackCount
    {
        public override Graphic SubGraphicFor(Thing thing)
        {
	        if (!TransparentThingsMod.settings.enableTreeTransparency)
	        {
		        return subGraphics[0];
	        }
            IntVec3 mousePos = GameComponent_TransparentThing.Instance.mousePosition;
            return thing.Position.DistanceTo(mousePos) > 20f ? subGraphics[0] : subGraphics[1];
        }
    }

    public class GameComponent_TransparentThing : GameComponent
    {
        public IntVec3 mousePosition;
        public static GameComponent_TransparentThing Instance;

        public GameComponent_TransparentThing(Game game)
        {
            Instance = this;
        }
        public override void GameComponentUpdate()
        {
            base.GameComponentTick();
            mousePosition = UI.MouseCell();
        }

    }
}
