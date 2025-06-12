using Verse;

namespace MedievalOverhaul
{
    public interface ICompFuelHandler
    {
        ThingFilter AllowedFuelFilter { get; }
    }

}
