using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

public class QuestPart_BOBHospitalUtilities : QuestPartActivable
{
    public MapParent mapParent;
    public string outSignal;
    private bool signalSent;
    private int hostilesActive;

    public override void QuestPartTick()
    {
        // only send once
        if (signalSent) return;

        // get map
        Map mapToCheck = mapParent?.Map;
        if (mapToCheck != null)
        {
            // map still exists
            // check if hostiles are still here
            hostilesActive = mapToCheck.mapPawns.AllPawnsSpawned.Count(p => !p.DeadOrDowned && p.HostileTo(Faction.OfPlayer));
        }
        else
        {
            // map is gone
            signalSent = true;

            // are hostiles still here?
            if (hostilesActive <= 0)
            {
                quest.End(QuestEndOutcome.Success);
            }
            else
            {
                quest.End(QuestEndOutcome.Fail);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref mapParent, "mapParent");
        Scribe_Values.Look(ref outSignal, "outSignal");
        Scribe_Values.Look(ref signalSent, "signalSent");
        Scribe_Values.Look(ref hostilesActive, "hostilesActive");
    }
}
