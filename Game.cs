using System.Windows.Forms;

namespace Digger
{
    public class Game
    {
        private const string mapWithPlayerTerrain = @"
TTT T
TTP T
T T T
TT TT";

        private const string mapWithPlayerTerrainSackGold = @"
PTTGTT TS
TST  TSTT
TTTTTTSTT
T TSTS TT
T TTTG ST
TSTSTT TT";

        private const string mapWithPlayerTerrainSackGoldMonster = @"
PTTGTT TST
TST  TSTTM
TTT TTSTTT
T TSTS TTT
T TTTGMSTS
T TMT M TS
TSTSTTMTTT
S TTST  TG
 TGST MTTT
 T  TMTTTT";

        public ICreature[,] Map;
        public int Scores;
        public bool IsOver;

        public Keys KeyPressed;
        public int MapWidth => Map.GetLength(0);
        public int MapHeight => Map.GetLength(1);
        
        private DiggerWindow _window;

        public void CreateMap()
        {
            var mapCreature = new CreatureMapCreator();
            Map = mapCreature.CreateMap(mapWithPlayerTerrainSackGoldMonster, this);
        }

        public void Run()
        {
            _window = new DiggerWindow(this);
            Application.Run(_window);
        }
    }
}