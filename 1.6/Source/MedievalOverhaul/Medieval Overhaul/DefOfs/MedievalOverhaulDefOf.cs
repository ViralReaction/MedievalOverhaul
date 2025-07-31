using Verse;
using RimWorld;
using ProcessorFramework;
using DefOf = RimWorld.DefOf;

namespace MedievalOverhaul
{
    [DefOf]
    public static class MedievalOverhaulDefOf
    {
        public static ThingDef DankPyon_Artillery_Trebuchet;
        public static ThingDef DankPyon_Turret_Trebuchet;
        public static ThingDef DankPyon_Artillery_Boulder;
        public static ThingDef DankPyon_CavalrySpike;
        public static ThingDef DankPyon_MealBread;

        public static TerrainDef DankPyon_PlowedSoil;
        public static DamageDef DankPyon_SchratCollapse;

        public static ThingDef DankPyon_Fat;
        public static ThingDef DankPyon_Bone;

        public static ThingCategoryDef DankPyon_Wood;
        public static ProcessDef DankPyon_RawHidesProcess;

        public static HediffDef DankPyon_UnpleasantAftermath;

        public static ThingCategoryDef DankPyon_Hides;
        public static ThingCategoryDef DankPyon_RawWood;

        public static PawnKindDef DankPyon_SchratDark_Sapling;

        public static ThingDef DankPyon_MineShaft;
        public static ThingDef DankPyon_ComponentBasic;

        public static JobDef DankPyon_Slop_Refuel_StatAtomic;
        public static JobDef DankPyon_Slop_Refuel_Stat;
        public static JobDef DankPyon_DoBillMending;
        public static JobDef DankPyon_Mine_Golem;
        public static JobDef DankPyon_OperateQuest;
        public static JobDef DankPyon_RefuelCustom;
        public static JobDef DankPyon_RefuelAtomicCustom;
        public static JobDef DankPyon_DispenseSlop;
        public static JobDef DankPyon_EmptyPot;
        public static JobDef DankPyon_IceMold_Empty;

        public static SoundDef MeleeHit_Wood;
        public static SoundDef BulletImpact_Wood;
        public static SoundDef Pawn_Melee_Punch_HitBuilding_Wood;

        public static HediffDef DankPyon_LindwurmAcidImmune;
        public static HediffDef DankPyon_StunImmune;
        public static HediffDef DankPyon_DazeImmune;

        public static ThingDef DankPyon_IceCellar;
        public static ThingDef DankPyon_IceBlock;
        public static ThingDef DankPyon_Book_ScribeTable;
        public static ThingDef DankPyon_Flour;
        public static ThingDef DankPyon_WaterMill;

        public static DamageDef DankPyon_AlpNightmare;
        public static StatDef RestFallRateFactor;
        public static TerrainAffordanceDef GrowSoil;

        public static ApparelLayerDef DankPyon_Hood;
        public static ApparelLayerDef DankPyon_InnerHelmet;

        public static ThingDef DankPyon_CultBook;
        public static FactionDef DankPyon_ShadowSect;

        public static ConceptDef DankPyon_Concept_Tanning;
        public static ConceptDef DankPyon_Concept_Lumber;
        public static ConceptDef DankPyon_Concept_ExplorerTable;
        public static ConceptDef DankPyon_Concept_Schematics;
        public static ConceptDef DankPyon_Concept_Regeneration;

        [MayRequireCE]
        public static StatDef Bulk;

        static MedievalOverhaulDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MedievalOverhaulDefOf));
    }

}
