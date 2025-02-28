using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    /// <summary>
    /// Market value stat part. Used to apply market value modifier for hides and contained leather.
    /// </summary>
    /// <seealso cref="RimWorld.StatPart" />
    internal class StatPart_MarketValue_Hide : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {

            if (req.HasThing)
            {
                if (HideUtility.IsHide(req.Thing.def))
                {
                    var comp = req.Thing.TryGetComp<CompGenericHide>();
                    if (comp != null)
                    {
                        if (comp.leatherAmount > comp.Props.leatherAmount)
                            val = comp.marketValue;
                    }
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            return null;
        }

    }
}