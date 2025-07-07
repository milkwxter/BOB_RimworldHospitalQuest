using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System;
using Verse;
using Verse.Grammar;

namespace Hospital_Rimworld
{
    class QuestNode_Root_Hospital : QuestNode
    {
        private bool TryFindSiteTile(out PlanetTile tile, bool exitOnFirstTileFound = false)
        {
            return TileFinder.TryFindNewSiteTile(out tile, 5, 15, allowCaravans: false, null, 0.5f, canSelectComboLandmarks: true, TileFinderMode.Near, exitOnFirstTileFound);
        }

        private Site GenerateSite(Slate slate)
        {
            // what is the enemy faction
            Faction raiders = Find.FactionManager.RandomRaidableEnemyFaction();

            // get the site part def
            SitePartDef sitePartDef = DefDatabase<SitePartDef>.GetNamed("BOB_Hospital");

            // find a suitable tile
            PlanetTile tile;
            TryFindSiteTile(out tile);

            // create the parameters
            SitePartParams sitePartParams = new SitePartParams
            {
                threatPoints = 100f
            };

            // create the site
            Site site = (Site)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Site);
            site.Tile = tile;
            site.SetFaction(raiders);
            site.AddPart(new SitePart(site, sitePartDef, sitePartParams));
            site.doorsAlwaysOpenForPlayerPawns = true;

            // add it to the world
            Find.WorldObjects.Add(site);

            // update the slate
            slate.Set("site", site);
            slate.Set("faction", site.Faction);

            // return our beautiful site so we can access it again
            return site;
        }

        protected override void RunInt()
        {
            // create the most important variables
            Slate slate = QuestGen.slate;
            Quest quest = QuestGen.quest;

            // add site to map and update slate
            Site site = GenerateSite(slate);

            // do the quest part
            QuestPart_SpawnWorldObject spawnPart = new QuestPart_SpawnWorldObject();
            spawnPart.worldObject = site;
            quest.AddPart(spawnPart);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }
}
