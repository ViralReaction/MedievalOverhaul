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
            ApplyPoisonEffect(user, pawnWeapon);
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

        private void ApplyPoisonEffect(Pawn pawn, ThingWithComps weapon)
        {
            CompPoisonWeapon poisonComp = weapon.TryGetComp<CompPoisonWeapon>();
            if (poisonComp == null)
            {
                poisonComp = new CompPoisonWeapon();
                poisonComp.hediffDef = Props.hediffDef;
                poisonComp.oilType = Props.oilType;
                weapon.AllComps.Add(poisonComp);
            }

            poisonComp.RefreshPoisonDuration();

            Messages.Message("MessagePoisonApplied".Translate(pawn.LabelShort, weapon.LabelShort), pawn, MessageTypeDefOf.PositiveEvent);
        }
    }
}
