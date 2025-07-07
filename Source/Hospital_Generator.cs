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
            for (int i = 0; i < medStacks; i++)
            {
                IntVec3 dropCell = room.Cells.Where(c => c.Standable(map)).InRandomOrder().FirstOrDefault();

                if (dropCell != IntVec3.Invalid)
                {
                    // get our loot table
                    ThingSetMakerDef lootDef = DefDatabase<ThingSetMakerDef>.GetNamed("BOB_Hospital_LootSet");
                    List<Thing> loot = lootDef.root.Generate(new ThingSetMakerParams());

                    // spawn item if the loot table has entries
                    if (loot.Any())
                    {
                        Thing med = loot.RandomElement();
                        GenPlace.TryPlaceThing(med, dropCell, map, ThingPlaceMode.Direct);
                    }
                }
            }
        }

        private void SpawnPirates(Room room)
        {
            // set vars
            int roomSize = room.CellCount;

            // create list of pirates to defend that room, immersive
            List<Pawn> piratesDefendingRoom = new List<Pawn>();

            // spawn pirates according to size
            int pirateCount = Math.Min(roomSize / 30, 3);
            for (int i = 0; i < pirateCount; i++)
            {
                PawnGenerationRequest req = new PawnGenerationRequest(
                    kind: PawnKindDefOf.Pirate,
                    faction: room.Map.ParentFaction,
                    context: PawnGenerationContext.NonPlayer,
                    tile: room.Map.Tile,
                    forceGenerateNewPawn: true,
                    allowDead: false
                );

                Pawn pirate = PawnGenerator.GeneratePawn(req);
                IntVec3 spawnCell = room.Cells.Where(c => c.Standable(room.Map)).InRandomOrder().FirstOrDefault();
                if (spawnCell != IntVec3.Invalid)
                {
                    GenSpawn.Spawn(pirate, spawnCell, room.Map);
                    piratesDefendingRoom.Add(pirate);
                }
            }

            // lord job so pirates dont leave and instead defend their room
            Lord pirateLord = LordMaker.MakeNewLord(room.Map.ParentFaction, new LordJob_DefendBase(room.Map.ParentFaction, room.Cells.RandomElement(), 25000, false), room.Map, piratesDefendingRoom);
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            // choose which layout to spawn
            var allLayouts = DefDatabase<KCSG.StructureLayoutDef>.AllDefsListForReading;
            var hospitalLayouts = allLayouts.Where(def => def.tags != null && def.tags.Contains("BOB_Hospital_Layout")).ToList();
            KCSG.StructureLayoutDef layoutDef = hospitalLayouts.RandomElement();

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
                }
            }

            // spawn medicine and pirates
            foreach (Room room in uniqueRooms)
            {
                // error handling
                if (room == null || room.TouchesMapEdge) continue;

                // get size of room
                int roomSize = room.CellCount;

                // spawn random medicine around
                SpawnMedicalLoot(room);

                // spawn a few pirates in that room
                SpawnPirates(room);
            }
        }
    }
}
