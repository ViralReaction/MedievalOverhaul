using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class CompUseEffect_AddOil : CompUseEffect
    {
        public ThingWithComps pawnWeapon;

        
        public new CompProperties_UseEffectAddOil Props
        {
            get
            {
                return (CompProperties_UseEffectAddOil)this.props;
            }
        }

        public override void DoEffect(Pawn user)
        {
            ApplyOil(user, pawnWeapon);
        }

        public override AcceptanceReport CanBeUsedBy(Pawn pawn)
        {
            if (pawn?.equipment?.Primary is ThingWithComps weapon && weapon.def.IsMeleeWeapon)
            {
                pawnWeapon = weapon;
                return true;
            }
            return false;
        }

        private void ApplyOil(Pawn pawn, ThingWithComps weapon)
        {
            CompWeaponOil oilComp = weapon.TryGetComp<CompWeaponOil>();
            if (oilComp == null)
            {
                oilComp = new CompWeaponOil();
                weapon.AllComps.Add(oilComp);
            }
            oilComp.RefreshOil(Props.maxCharges, Props.oilType, Props.hediffDef, Props.oilRefillDef);
            OiledWeaponsComponent comp = OiledWeaponsManager.GetComp();
            comp.RegisterOilWeapon(weapon, oilComp);
        }
    }
}
