using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Digger
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            var actions = new Action[] { Run, Run };
            Parallel.Invoke(actions);
        }

        private static void Run()
        {
            var game = new Game();
            game.CreateMap();
            game.Run();
        }
    }
}