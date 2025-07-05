using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace Hospital_Rimworld
{
    class QuestNode_Root_Hospital : QuestNode
    {
        private bool TryFindSiteTile(out PlanetTile tile, bool exitOnFirstTileFound = false)
        {
            return TileFinder.TryFindNewSiteTile(out tile, 5, 15, allowCaravans: false, null, 0.5f, canSelectComboLandmarks: true, TileFinderMode.Near, exitOnFirstTileFound);
        }

        protected override void RunInt()
        {
            // create the most important variables
            Slate slate = QuestGen.slate;

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

            // finally do the quest part
            QuestPart_SpawnWorldObject spawnPart = new QuestPart_SpawnWorldObject();
            spawnPart.worldObject = site;
            QuestGen.quest.AddPart(spawnPart);
        }

        protected override bool TestRunInt(Slate slate)
        {
            // always work
            return true;
        }
    }
}
