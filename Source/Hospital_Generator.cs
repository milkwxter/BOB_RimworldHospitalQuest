using KCSG;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace Hospital_Rimworld
{
    public class GenStep_SpawnHospitalLayout : GenStep
    {
        public override int SeedPart => 901238893;

        private void SpawnMedicalLoot(Room room)
        {
            // set vars
            int roomSize = room.CellCount;
            Map map = room.Map;

            // spawn medicine according to size
            int medStacks = Math.Min(roomSize / 20, 2);
            Log.Message($"medStacks: {medStacks}");
            for (int i = 0; i < medStacks; i++)
            {
                IntVec3 dropCell = room.Cells.Where(c => c.Standable(map)).InRandomOrder().FirstOrDefault();

                var standableCells = room.Cells.Where(c => c.Standable(map)).ToList();
                Log.Message($"Room size: {room.CellCount}, Standable cells: {standableCells.Count}");

                if (dropCell != IntVec3.Invalid)
                {
                    // get our loot table
                    ThingSetMakerDef lootDef = DefDatabase<ThingSetMakerDef>.GetNamed("BOB_Hospital_LootSet");
                    List<Thing> loot = lootDef.root.Generate(new ThingSetMakerParams());

                    Log.Message($"LootDef found: {lootDef?.defName}");
                    Log.Message($"Generated loot count: {loot.Count}");

                    // spawn item if the loot table has entries
                    if (loot.Any())
                    {
                        Thing med = loot.RandomElement();
                        GenPlace.TryPlaceThing(med, dropCell, map, ThingPlaceMode.Direct);
                    }
                }
            }
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            // choose which layout to spawn
            KCSG.StructureLayoutDef layoutDef = DefDatabase<KCSG.StructureLayoutDef>.GetNamed("BOB_HospitalTest");

            // spawn the layout
            Faction parentFaction = map.ParentFaction;
            IntVec3 center = map.Center;
            IntVec2 sizes = layoutDef.Sizes;
            CellRect cellRect = CellRect.CenteredOn(center, sizes);
            GenOption.GetAllMineableIn(cellRect, map);
            layoutDef.Generate(cellRect, map, parentFaction);

            // save the rooms so we can play with them
            List<Room> uniqueRooms = new List<Room>();
            foreach (IntVec3 cell in cellRect)
            {
                Room room = cell.GetRoom(map);
                if (room != null && !uniqueRooms.Contains(room))
                {
                    uniqueRooms.Add(room);
                    Log.Message("[BOB Hospital Quest] Found a unique room!");
                }
            }

            // spawn medicine and pirates
            List<Pawn> allMyPirates = new List<Pawn>();
            foreach (Room room in uniqueRooms)
            {
                // error handling
                if (room == null || room.TouchesMapEdge) continue;

                // get size of room
                int roomSize = room.CellCount;

                // spawn random medicine around
                SpawnMedicalLoot(room);

                // spawn pirates according to size
                int pirateCount = Math.Min(roomSize / 30, 3);
                for (int i = 0; i < pirateCount; i++)
                {
                    PawnGenerationRequest req = new PawnGenerationRequest(
                        kind: PawnKindDefOf.Pirate,
                        faction: parentFaction,
                        context: PawnGenerationContext.NonPlayer,
                        tile: map.Tile,
                        forceGenerateNewPawn: true,
                        allowDead: false
                    );

                    Pawn pirate = PawnGenerator.GeneratePawn(req);
                    IntVec3 spawnCell = room.Cells.Where(c => c.Standable(map)).InRandomOrder().FirstOrDefault();
                    if (spawnCell != IntVec3.Invalid)
                    {
                        GenSpawn.Spawn(pirate, spawnCell, map);
                        allMyPirates.Add(pirate);
                    }
                }
            }

            // lord job so pirates dont leave
            IntVec3 defendSpot = cellRect.CenterCell;
            Lord pirateLord = LordMaker.MakeNewLord(parentFaction, new LordJob_DefendBase(parentFaction, defendSpot, 25000, false), map, allMyPirates);
        }
    }
}
