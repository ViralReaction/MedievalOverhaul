using RimWorld;
using UnityEngine;
using Verse;

namespace TransparentThings
{
    public class Graphic_TransparentThing : Graphic_StackCount
    {
        public override Graphic SubGraphicFor(Thing thing)
        {
            IntVec3 mousePos = GameComponent_TransparentThing.Instance.mousePosition;
            return thing.Position.DistanceTo(mousePos) > 20f ? subGraphics[0] : subGraphics[1];
        }
        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
	        Vector2 size;
	        bool flag;
	        if (ShouldDrawRotated)
	        {
		        size = drawSize;
		        flag = false;
	        }
	        else
	        {
		        size = (thing.Rotation.IsHorizontal ? drawSize.Rotated() : drawSize);
		        flag = (thing.Rotation == Rot4.West && WestFlipped) || (thing.Rotation == Rot4.East && EastFlipped);
	        }
	        if (thing.MultipleItemsPerCellDrawn())
	        {
		        size *= 0.8f;
	        }
	        float num = AngleFromRot(thing.Rotation) + extraRotation;
	        if (flag && data != null)
	        {
		        num += data.flipExtraRotation;
	        }
	        Vector3 center = thing.TrueCenter() + DrawOffset(thing.Rotation);
	        Material material = MatAt(thing.Rotation, thing);
	        TryGetTextureAtlasReplacementInfo(material, thing.def.category.ToAtlasGroup(), flag, vertexColors: true, out material, out var uvs, out var vertexColor);
	        Printer_Plane.PrintPlane(layer, center, new Vector2(10f,10f), material, num, flag, uvs, new Color32[4]
	        {
		        vertexColor, vertexColor, vertexColor, vertexColor
	        });
	        ShadowGraphic?.Print(layer, thing, 0f);
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
