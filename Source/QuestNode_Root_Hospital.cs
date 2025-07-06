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

        private Site GenerateSite(Slate slate, out string siteMapGeneratedSignal, out string siteMapRemovedSignal)
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

            // create signals for this site
            siteMapRemovedSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
            siteMapGeneratedSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");

            // return our beautiful site so we can access it again
            return site;
        }

        protected override void RunInt()
        {
            // create the most important variables
            Slate slate = QuestGen.slate;
            Quest quest = QuestGen.quest;

            // add site to map and update slate
            Site site = GenerateSite(slate, out var siteMapGeneratedSignal, out var siteMapRemovedSignal);

            // do the quest part
            QuestPart_SpawnWorldObject spawnPart = new QuestPart_SpawnWorldObject();
            spawnPart.worldObject = site;
            quest.AddPart(spawnPart);

            // simple quest signal
            string siteMapEnemiesDefeatedSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.AllEnemiesDefeated");

            // Combine both signals
            QuestPart_PassAll passAll = new QuestPart_PassAll();
            passAll.inSignals.Add(siteMapEnemiesDefeatedSignal);
            passAll.inSignals.Add(siteMapRemovedSignal);
            passAll.outSignal = "site.BothConditionsMet";
            quest.AddPart(passAll);

            // quest success condition
            quest.Message("You defeated the enemies! Now steal some stuff and run!", MessageTypeDefOf.PositiveEvent, false, null, null, siteMapEnemiesDefeatedSignal);
            quest.SignalPassActivable(delegate {
                quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
            }, siteMapGeneratedSignal, "site.BothConditionsMet");

            // quest failure condition // this one always activates FUCK!!!!!!!!
            quest.SignalPassActivable(delegate {
                quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
            }, siteMapGeneratedSignal, siteMapRemovedSignal);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }
}
