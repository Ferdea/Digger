using System.Collections.Generic;
using System.Windows.Forms;

namespace Digger
{
    public class Terrain : ICreature
    {
        private Game Game { get; }

        public Terrain(Game game)
        {
            Game = game;
        }
        
        public string GetImageFileName()
        {
            return "Terrain.png";
        }

        public int GetDrawingPriority()
        {
            return 10;
        }

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Player;
        }
    }

    public class Player : ICreature
    {
        private Game Game { get; }

        public Player(Game game)
        {
            Game = game;
        }
        
        public string GetImageFileName()
        {
            return "Digger.png";
        }

        public int GetDrawingPriority()
        {
            return 5;
        }

        private CreatureCommand GetMovingFromPressedKeys()
        {
            switch (Game.KeyPressed)
            {
                case Keys.Up:
                    return new CreatureCommand { DeltaY = -1 };
                case Keys.Down:
                    return new CreatureCommand { DeltaY = 1 };
                case Keys.Left:
                    return new CreatureCommand { DeltaX = -1 };
                case Keys.Right:
                    return new CreatureCommand { DeltaX = 1 };
                default:
                    return new CreatureCommand();
            };
        }
        
        public CreatureCommand Act(int x, int y)
        {
            var command = GetMovingFromPressedKeys();
            var newX = x + command.DeltaX;
            var newY = y + command.DeltaY;
            if (0 <= newX && newX < Game.MapWidth && 0 <= newY && newY < Game.MapHeight &&
                !(Game.Map[newX, newY] is Sack))
            {
                return command;
            }

            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (!(conflictedObject is Sack || conflictedObject is Monster)) return false;
            Game.IsOver = true;
            return true;
        }
    }

    public class Sack : ICreature
    {
        private Game Game { get; }

        private int _dropDistance;

        public Sack(Game game)
        {
            Game = game;
        }
        
        public string GetImageFileName()
        {
            return "Sack.png";
        }

        public int GetDrawingPriority()
        {
            return 15;
        }

        public CreatureCommand Act(int x, int y)
        {
            if (y < Game.MapHeight - 1 &&
                (Game.Map[x, y + 1] is null || 
                 ((Game.Map[x, y + 1] is Player || 
                   Game.Map[x, y + 1] is Monster) && 
                  _dropDistance != 0)))
            {
                _dropDistance += 1;
                return new CreatureCommand { DeltaY = 1 };
            }

            if (_dropDistance > 1)
                return new CreatureCommand { TransformTo = new Gold(Game) };
            _dropDistance = 0;
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }
    }

    public class Gold : ICreature
    {
        private Game Game { get; }

        public Gold(Game game)
        {
            Game = game;
        }
        
        public string GetImageFileName()
        {
            return "Gold.png";
        }

        public int GetDrawingPriority()
        {
            return 10;
        }

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Player)
            {
                Game.Scores += 10;
                return true;
            }

            return conflictedObject is Monster;
        }
    }

    public class Monster : ICreature
    {
        private Game Game { get; }

        public Monster(Game game)
        {
            Game = game;
        }

        public string GetImageFileName()
        {
            return "Monster.png";
        }

        public int GetDrawingPriority()
        {
            return 5;
        }

        private bool IsCorrectMove((int Length, int X, int Y) currentCell, (int DX, int DY) direction, int[,] pathMap)
        {
            return 0 <= currentCell.X + direction.DX && currentCell.X + direction.DX < Game.MapWidth &&
                   0 <= currentCell.Y + direction.DY && currentCell.Y + direction.DY < Game.MapHeight &&
                   pathMap[currentCell.X + direction.DX, currentCell.Y + direction.DY] > currentCell.Length + 1 &&
                   !(Game.Map[currentCell.X + direction.DX, currentCell.Y + direction.DY] is Terrain ||
                     Game.Map[currentCell.X + direction.DX, currentCell.Y + direction.DY] is Sack ||
                     Game.Map[currentCell.X + direction.DX, currentCell.Y + direction.DY] is Monster);
        }

        private int FindPath(int[,] pathMap, Stack<(int Length, int X, int Y)> cellsStack,
            Stack<(int Length, int X, int Y)> path, (int Length, int X, int Y)[] bestPath,
            (int DX, int DY)[] directions, int bestPathLength = 0)
        {
            while (cellsStack.Count > 0)
            {
                if (path.Count > 0 && path.Peek().Length >= cellsStack.Peek().Length)
                {
                    path.Pop();
                    continue;
                }

                var currentCell = cellsStack.Pop();
                path.Push(currentCell);
                pathMap[currentCell.X, currentCell.Y] = currentCell.Length;

                if (Game.Map[currentCell.X, currentCell.Y] is Player &&
                    (path.Count < bestPathLength || bestPathLength == 0))
                {
                    path.CopyTo(bestPath, 0);
                    bestPathLength = path.Count;
                    continue;
                }

                foreach (var d in directions)
                    if (IsCorrectMove(currentCell, d, pathMap))
                        cellsStack.Push((currentCell.Length + 1, currentCell.X + d.DX, currentCell.Y + d.DY));
            }

            return bestPathLength;
        }

        private CreatureCommand FindMove(int x, int y)
        {
            var pathMap = new int[Game.MapWidth, Game.MapHeight];
            for (var i = 0; i < Game.MapWidth; i++)
            for (var j = 0; j < Game.MapHeight; j++)
                pathMap[i, j] = int.MaxValue;
            var cellsStack = new Stack<(int Length, int X, int Y)>();
            cellsStack.Push((0, x, y));
            var path = new Stack<(int Length, int X, int Y)>();
            var bestPath = new (int Length, int X, int Y)[Game.MapHeight * Game.MapWidth];
            var directions = new (int DX, int DY)[]{(1, 0), (-1, 0), (0, 1), (0, -1)};
            
            var bestPathLength = FindPath(pathMap, cellsStack, path, bestPath, directions);

            if (bestPathLength <= 1) return new CreatureCommand();
            var move = bestPath[bestPathLength - 2];
            return new CreatureCommand { DeltaX = move.X - x, DeltaY = move.Y - y };
        }
        
        public CreatureCommand Act(int x, int y)
        {
            return FindMove(x, y);
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Sack || conflictedObject is Monster;
        }
    }
}