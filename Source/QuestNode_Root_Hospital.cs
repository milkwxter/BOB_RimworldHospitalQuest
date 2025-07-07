using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System;
using Verse;
using Verse.Grammar;
using Verse.Noise;

namespace Hospital_Rimworld
{
    class QuestNode_Root_Hospital : QuestNode
    {
        private bool TryFindSiteTile(out PlanetTile tile, bool exitOnFirstTileFound = false)
        {
            return TileFinder.TryFindNewSiteTile(
                out tile,
                minDist: 5,
                maxDist: 8,
                allowedLandmarks: null,
                selectLandmarkChance: 0f,
                canSelectComboLandmarks: false,
                tileFinderMode: TileFinderMode.Near,
                exitOnFirstTileFound: true,
                canBeSpace: false,
                validator: x => Find.WorldGrid[x].hilliness == Hilliness.Flat
            );
        }

        private Site GenerateSite(Quest quest, Slate slate)
        {
            // what is the enemy faction
            Faction raiders = Find.FactionManager.RandomRaidableEnemyFaction();

            // find my sitepart
            SitePartDef sitePartDef = DefDatabase<SitePartDef>.GetNamed("BOB_Hospital");

            // find a suitable tile
            TryFindSiteTile(out PlanetTile tile);

            // create the parameters
            SitePartParams sitePartParams = new SitePartParams
            {
                threatPoints = 100f
            };

            // create the site
            Site site = QuestGen_Sites.GenerateSite((IEnumerable<SitePartDefWithParams>)new List<SitePartDefWithParams>
            {
                new SitePartDefWithParams(sitePartDef, sitePartParams)
            }, tile, raiders, false, (RulePack)null);
            site.doorsAlwaysOpenForPlayerPawns = true;

            // spawn the world object we created
            quest.SpawnWorldObject(site);

            // return our beautiful site so we can access it again
            return site;
        }

        protected override void RunInt()
        {
            // create the most important variables
            Slate slate = QuestGen.slate;
            Quest quest = QuestGen.quest;

            // create the site
            Site site = GenerateSite(quest, slate);

            // update slate
            slate.Set("playerFaction", Faction.OfPlayer);
            slate.Set("map", QuestGen_Get.GetMap(false, (int?)null));
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }
}
