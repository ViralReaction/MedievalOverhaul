using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MedievalOverhaul
{
    public class CompUsablePoisonOil : CompUsable
    {
        public override string FloatMenuOptionLabel(Pawn pawn)
        {
            if (pawn?.equipment?.Primary is ThingWithComps weapon && weapon.def.IsMeleeWeapon)
            {
                return "DankPyon_ApplyPoisonOil".Translate(weapon.LabelShort);
            }
            return "DankPyon_CannotApplyPoisonOil".Translate();
        }

        public override void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget, bool forced = false)
        {
            if (pawn?.equipment?.Primary is ThingWithComps weapon && weapon.def.IsMeleeWeapon)
            {
                UseJobInternal(pawn, extraTarget);
            }
            else
            {
                Messages.Message("MessageMustHaveMeleeWeapon".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.RejectInput);
            }
        }

        private void UseJobInternal(Pawn pawn, LocalTargetInfo extraTarget)
        {
            base.TryStartUseJob(pawn, extraTarget, false);
        }

    }

}