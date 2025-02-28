using RimWorld;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public class Building_Art_Ice : Building_Art
    {
        private Graphic _cacheGraphic;
        public override Graphic Graphic
        {
            get
            {
                if ( _cacheGraphic is not null )
                {
                    return _cacheGraphic;
                }
                var graphic = base.Graphic;
                Shader shader = ShaderDatabase.TransparentPostLight;
                Color newColor = new Color(graphic.Color.r, graphic.Color.g, graphic.Color.b, 0.50f);
                graphic = graphic.GetColoredVersion(shader, newColor, graphic.ColorTwo);
                _cacheGraphic = graphic;
                return _cacheGraphic;
            }
        }

    }
}
