using Verse;
namespace MedievalOverhaul
{
    public class DeathSplitExtension : DefModExtension
    {
        public PawnKindDef pawnKindDef;
        public IntRange createAmount = new IntRange(1);
        public float randomChance = 1f;
        public bool explosion = false;
        public DamageDef damageDef;
        public float explosionRadius = 3f;
        public int explosionDamage = 10;
        public float armorPen = -1f;

    }
}
