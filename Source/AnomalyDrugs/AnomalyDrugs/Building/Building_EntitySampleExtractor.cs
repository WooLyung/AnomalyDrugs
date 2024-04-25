using RimWorld;
using System.Collections.Generic;
using System;
using Verse;
using Verse.Sound;
using UnityEngine;
using UnityEngine.Profiling;

namespace AnomalyDrugs
{
    public class Building_EntitySampleExtractor : Building
    {
        public bool unloadingEnabled = true;
        private bool initalized;
        private CompFacility facilityComp;
        private CompPowerTrader powerComp;
        private CompCableConnection cableConnection;
        private Sustainer workingSustainer;

        public CompFacility FacilityComp => this.facilityComp ?? (this.facilityComp = this.GetComp<CompFacility>());

        public CompPowerTrader Power => this.powerComp ?? (this.powerComp = this.GetComp<CompPowerTrader>());

        public List<Thing> Platforms => this.FacilityComp.LinkedBuildings;

        public CompCableConnection CableConnection => this.cableConnection ?? (this.cableConnection = this.GetComp<CompCableConnection>());

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad)
                return;
            this.Initialize();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            this.FacilityComp.OnLinkAdded -= new Action<CompFacility, Thing>(this.OnLinkAdded);
            this.FacilityComp.OnLinkRemoved -= new Action<CompFacility, Thing>(this.OnLinkRemoved);
            this.initalized = false;
            this.workingSustainer?.End();
            this.workingSustainer = (Sustainer)null;
        }

        private void Initialize()
        {
            if (this.initalized)
                return;
            this.initalized = true;
            this.FacilityComp.OnLinkAdded += new Action<CompFacility, Thing>(this.OnLinkAdded);
            this.FacilityComp.OnLinkRemoved += new Action<CompFacility, Thing>(this.OnLinkRemoved);
            foreach (Thing platform in this.Platforms)
            {
                if (platform is Building_HoldingPlatform buildingHoldingPlatform)
                    buildingHoldingPlatform.innerContainer.OnContentsChanged += new Action(this.RebuildCables);
            }
            this.RebuildCables();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (this.initalized)
                return;
            this.Initialize();
        }

        private void OnLinkRemoved(CompFacility facility, Thing thing)
        {
            if (!(thing is Building_HoldingPlatform buildingHoldingPlatform))
                return;
            buildingHoldingPlatform.innerContainer.OnContentsChanged -= new Action(this.RebuildCables);
            this.RebuildCables();
        }

        private void OnLinkAdded(CompFacility facility, Thing thing)
        {
            if (!(thing is Building_HoldingPlatform buildingHoldingPlatform))
                return;
            buildingHoldingPlatform.innerContainer.OnContentsChanged += new Action(this.RebuildCables);
            this.RebuildCables();
        }

        public override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(15000))
                EjectSample();
        }

        private Pawn GetTarget()
        {
            List<Pawn> targets = new List<Pawn>();
            foreach (Thing platform in this.Platforms)
            {
                if (platform is Building_HoldingPlatform buildingHoldingPlatform && buildingHoldingPlatform.Occupied)
                    targets.Add(buildingHoldingPlatform.HeldPawn);
            }

            if (targets.Any())
                return targets.RandomElement();
            return null;
        }

        private void EjectSample()
        {
            if (!Power.PowerOn)
                return;

            Pawn target = GetTarget();
            if (target == null)
                return;
            
            ThingDef sampleDef = null;
            PawnKindDef kind = target.kindDef;

            if (kind == PawnKindDefOf.Metalhorror)
                sampleDef = DefDatabase<ThingDef>.GetNamed("MetalhorrorEntitySample", false);
            else if (kind == PawnKindDefOf.Sightstealer)
                sampleDef = DefDatabase<ThingDef>.GetNamed("SightstealerEntitySample", false);
            else if (kind == PawnKindDefOf.Revenant)
                sampleDef = DefDatabase<ThingDef>.GetNamed("RevenantEntitySample", false);

            if (sampleDef != null)
            {
                target.TakeDamage(new DamageInfo(DamageDefOf.Cut, 5));
                Thing sample = ThingMaker.MakeThing(sampleDef);
                GenPlace.TryPlaceThing(sample, this.Position, this.Map, ThingPlaceMode.Near);
            }
        }

        public override void Notify_DefsHotReloaded()
        {
            base.Notify_DefsHotReloaded();
            this.RebuildCables();
        }

        private void RebuildCables() => this.CableConnection.RebuildCables(this.Platforms, (Func<Thing, bool>)(thing => thing is Building_HoldingPlatform buildingHoldingPlatform && buildingHoldingPlatform.Occupied));

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
