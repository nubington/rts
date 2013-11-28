using System.Collections.Generic;

namespace rts
{
    public class Player
    {
        public static Player[] Players = new Player[2];
        public static Player Me;

        public PlayerStats Stats = new PlayerStats();

        static Player()
        {
            for (short i = 0; i < Players.Length; i++)
            {
                Players[i] = new Player(i);
            }
        }

        public short Team;
        public int Roks;
        public int CurrentSupply;
        public int MaxSupply;

        public List<ScheduledAction> ScheduledActions = new List<ScheduledAction>();

        public Unit[] UnitArray = new Unit[2048];
        public Structure[] StructureArray = new Structure[2048];
        public List<UnitCommand> UnitCommands = new List<UnitCommand>();

        public short UnitIDCounter;
        public short CommandIDCounter;
        public short StructureIDCounter;

        Player(short team)
        {
            Team = team;
        }
    }
}
