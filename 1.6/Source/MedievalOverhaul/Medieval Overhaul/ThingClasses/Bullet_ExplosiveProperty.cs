﻿using RimWorld;
using Verse;

namespace MedievalOverhaul
{

    public class Bullet_ExplosiveProperty : Bullet
    {

        private ProjectileProperties AdditionalProjProps => (def.GetModExtension<AdditionalProjectileProperties>() ?? AdditionalProjectileProperties.defaultValues).projectile2;

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            var usedTargetInfo = hitThing ?? new TargetInfo(usedTarget.Cell, Map);
            DoExplosion(usedTargetInfo);
            base.Impact(hitThing);
        }

        private void DoExplosion(TargetInfo targetInfo)
        {
            var projProps = AdditionalProjProps;

            var map = Map;
            if (projProps.explosionEffect != null)
            {
                var effecter = projProps.explosionEffect.Spawn();
                effecter.Trigger(targetInfo, targetInfo);
                effecter.Cleanup();
            }

            var centre = targetInfo.Cell;
            float radius = projProps.explosionRadius;
            var damType = projProps.damageDef;
            var instigator = launcher;
            int damAmount = projProps.GetDamageAmount(this);
            float armourPenetration = projProps.GetArmorPenetration(this);
            var explosionSound = projProps.soundExplode;
            var weapon = equipmentDef;
            var projectile = def;
            var intendedTarget = this.intendedTarget.Thing;
            var postExplosionSpawnThingDef = projProps.postExplosionSpawnThingDef;
            float postExplosionSpawnChance = projProps.postExplosionSpawnChance;
            int postExplosionSpawnThingCount = projProps.postExplosionSpawnThingCount;
            bool applyDamageToExplosionCellsNeighbours = projProps.applyDamageToExplosionCellsNeighbors;
            var preExplosionSpawnThingDef = projProps.preExplosionSpawnThingDef;
            float preExplosionSpawnChance = projProps.preExplosionSpawnChance;
            int preExplosionSpawnThingCount = projProps.preExplosionSpawnThingCount;
            float chanceToStartFire = projProps.explosionChanceToStartFire;
            bool damageFalloff = projProps.explosionDamageFalloff;

            GenExplosion.DoExplosion(centre, map, radius, damType, instigator, damAmount, armourPenetration, explosionSound, weapon, projectile, intendedTarget, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, null, null, 255,
                applyDamageToExplosionCellsNeighbours, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff);
        }

    }

}
