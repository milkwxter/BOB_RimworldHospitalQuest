using KCSG;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace Hospital_Rimworld
{
    public class GenStep_SpawnHospitalLayout : GenStep
    {
        public override int SeedPart => 901238893;

        public override void Generate(Map map, GenStepParams parms)
        {
            // choose which layout to spawn
            KCSG.StructureLayoutDef layoutDef = DefDatabase<KCSG.StructureLayoutDef>.GetNamed("m_TestHospital");

            // spawn the layout
            Faction parentFaction = map.ParentFaction;
            IntVec3 center = map.Center;
            IntVec2 sizes = layoutDef.Sizes;
            IntVec3 intVec = center + new IntVec3(-sizes.x, 0, 0);
            CellRect cellRect = CellRect.CenteredOn(intVec, sizes);
            GenOption.GetAllMineableIn(cellRect, map);
            layoutDef.Generate(cellRect, map, parentFaction);

            // save the rooms so we can play with them
            List<Room> uniqueRooms = new List<Room>();
            foreach (IntVec3 cell in map.AllCells)
            {
                Room room = cell.GetRoom(map);
                if (room != null && !uniqueRooms.Contains(room))
                {
                    uniqueRooms.Add(room);
                    Log.Message("Found a unique room!");
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

                // spawn medicine according to size
                int medStacks = Math.Min(roomSize / 20, 2);
                for (int i = 0; i < medStacks; i++)
                {
                    IntVec3 dropCell = room.Cells.Where(c => c.Standable(map)).InRandomOrder().FirstOrDefault();
                    if (dropCell != IntVec3.Invalid)
                    {
                        Thing med = ThingMaker.MakeThing(ThingDefOf.MedicineIndustrial);
                        med.stackCount = Rand.RangeInclusive(1, 3);
                        GenPlace.TryPlaceThing(med, dropCell, map, ThingPlaceMode.Direct);
                    }
                }

                // spawn pirates according to size
                int pirateCount = Math.Min(roomSize / 30, 3); // up to 3 pirates in large rooms
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
