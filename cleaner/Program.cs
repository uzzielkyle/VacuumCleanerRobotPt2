using System;

namespace RobotCleaner
{
    public class Map
    {
        private enum CellType { Empty, Dirt, Obstacle, Cleaned };
        private CellType[,] _grid;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Map(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            _grid = new CellType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _grid[x, y] = CellType.Empty;
                }
            }
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < this.Width && y >= 0 && y < this.Height;
        }

        public bool IsDirt(int x, int y)
        {
            return IsInBounds(x, y) && _grid[x, y] == CellType.Dirt;
        }

        public bool IsObstacle(int x, int y)
        {
            return IsInBounds(x, y) && _grid[x, y] == CellType.Obstacle;
        }

        public void AddObstacle(int x, int y)
        {
            _grid[x, y] = CellType.Obstacle;
        }
        public void AddDirt(int x, int y)
        {
            _grid[x, y] = CellType.Dirt;
        }

        public void Clean(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                _grid[x, y] = CellType.Cleaned;
            }
        }
        public void Display(int robotX, int robotY)
        {
            // display the 2d grid, it accepts the location of the robot in x and y
            Console.Clear();
            Console.WriteLine("Vacuum cleaner robot simulation");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Legends: #=Obstacles, D=Dirt, .=Empty, R=Robot, C=Cleaned");

            //display the grid using loop
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    if (x == robotX && y == robotY)
                    {
                        Console.Write("R ");
                    }
                    else
                    {
                        switch (_grid[x, y])
                        {
                            case CellType.Empty: Console.Write(". "); break;
                            case CellType.Dirt: Console.Write("D "); break;
                            case CellType.Obstacle: Console.Write("# "); break;
                            case CellType.Cleaned: Console.Write("C "); break;
                        }
                    }
                }
                Console.WriteLine();
            } //outer for loop
              // add delay
            Thread.Sleep(200);
        } // display method
    }//class map
    public interface IStrategy
    {
        void Clean(Robot robot);
    }

    public class Robot
    {
        private readonly Map _map;
        private readonly IStrategy _strategy;

        public int X { get; set; }
        public int Y { get; set; }

        public Map Map { get { return _map; } }

        public Robot(Map map, IStrategy strategy)
        {
            _map = map;
            _strategy = strategy;
            X = 0;
            Y = 0;
        }

        public bool Move(int newX, int newY)
        {
            if (_map.IsInBounds(newX, newY) && !_map.IsObstacle(newX, newY))
            {
                // set the new location
                X = newX;
                Y = newY;
                // display the map with the robot in its location in the grid
                _map.Display(X, Y);
                return true;
            }
            // it cannot move
            return false;
        }// Move method

        public void CleanCurrentSpot()
        {
            if (_map.IsDirt(X, Y))
            {
                _map.Clean(X, Y);
                _map.Display(X, Y);
            }
        }

        public void StartCleaning()
        {
            _strategy.Clean(this);
        }
    }

    public class ZigzagStrategy : IStrategy
    {
        public void Clean(Robot robot)
        {
            int direction = 1; // 1 = right, -1 = left
            for (int y = 0; y < robot.Map.Height; y++)
            {
                int startX = (direction == 1) ? 0 : robot.Map.Width - 1;
                int endX = (direction == 1) ? robot.Map.Width : -1;

                for (int x = startX; x != endX; x += direction)
                {
                    robot.Move(x, y);
                    robot.CleanCurrentSpot();
                }
                direction *= -1; // Reverse direction for the next row
            }
        }
    }

    public class SpiralStrategy : IStrategy
    {
        public void Clean(Robot robot)
        {
            int[,] directions = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
            int segmentLength = 1;
            int direction = 0;

            int startX = (robot.Map.Width - 1) / 2;
            int startY = (robot.Map.Height - 1) / 2;

            int posX = startX;
            int posY = startY;

            robot.Move(posX, posY);
            robot.CleanCurrentSpot();

            while (segmentLength <= Math.Min(robot.Map.Width, robot.Map.Height))
            {
                int step = 0;
                while (step < segmentLength)
                {
                    posX += directions[direction, 0];
                    posY += directions[direction, 1];

                    robot.Move(posX, posY);
                    robot.CleanCurrentSpot();

                    step++;
                }

                direction = (direction + 1) % 4;
                if (direction % 2 == 0) segmentLength++;
            }
        }
    }

    public class Program
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("Initialize robot");


            IStrategy zigzag_strategy = new ZigzagStrategy();
            IStrategy spiral_strategy = new SpiralStrategy();

            Map map = new Map(9, 9);
            // map.Display( 10,10);

            map.AddDirt(5, 3);
            map.AddDirt(1, 8);
            map.AddObstacle(8, 4);
            map.AddDirt(8, 8);

            Robot robot = new Robot(map, spiral_strategy);

            robot.StartCleaning();

            Console.WriteLine("Done.");
        }
    }
}

