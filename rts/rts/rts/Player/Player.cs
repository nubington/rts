using System.Collections.Generic;

namespace rts
{
    public class Player
    {
        public static Player[] Players = new Player[2];

        // Me is assigned by Rts gamestate constructor
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

        // units or structures that have died to be set null in arrays after some delay
        public List<KeyValuePair<short, float>> UnitIDsToSetNull = new List<KeyValuePair<short, float>>();
        public List<KeyValuePair<short, float>> StructureIDsToSetNull = new List<KeyValuePair<short, float>>();

        Player(short team)
        {
            Team = team;
        }

        // set IDs to null references if theyve reached delay time
        const float NULL_ID_DELAY = 10f;
        public static void SetNullIDS()
        {
            foreach (Player player in Players)
            {
                foreach (KeyValuePair<short, float> pair in player.UnitIDsToSetNull)
                {
                    if (pair.Value + NULL_ID_DELAY >= Rts.GameClock)
                        player.UnitArray[pair.Key] = null;
                }

                foreach (KeyValuePair<short, float> pair in player.StructureIDsToSetNull)
                {
                    if (pair.Value + NULL_ID_DELAY >= Rts.GameClock)
                        player.StructureArray[pair.Key] = null;
                }
            }
        }
    }
}
