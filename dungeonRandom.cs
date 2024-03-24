using System;
using System.ComponentModel;
using System.Xml;

namespace DungeonRandom
{
    enum Direction
    {
        none, north, east, south, west,
    }
    enum Size
    {
        // Canvas Sizes should be multiplicants of 2 
        canvasW = 80, canvasH = 20, maxRoomSize = 10, minRoomSize = 3,
    }
    enum Tile
    {        
        blank, wall, door, floor, corridor,
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Set grid to Blank Space
            int[,] grid = new int[(int)Size.canvasH, (int)Size.canvasW];
            foreach (int i in grid)
            {
                foreach (int j in grid)
                {
                    grid[i,j] = (int)Tile.blank;
                }
            }

            // Create starting point and set to middle 
            int[] start = {(int)Size.canvasH / 2, (int)Size.canvasW / 2};

            // Set direction to none
            int direction = (int)Direction.none;

            // Copy Grid into working Grid
            int[,] workGrid = new int[(int)Size.canvasH, (int)Size.canvasW];
            CopyGridToWorkGrid(grid, workGrid);

            // Generate a random Room 
            GenerateRoom(workGrid, start, direction);

            // Print Dungeon.
            PrintDungeon((int)Size.canvasH, (int)Size.canvasW, workGrid);
        }
        static int[,] CopyGridToWorkGrid(int[,] grid, int[,] wGrid)
        {
            foreach (int i in grid)
            {
                foreach (int j in grid)
                {
                    wGrid[i,j] = grid[i,j];
                }
            }
            return wGrid;
        }
        static int[,] GenerateRoom(int[,] wGrid, int[] start, int direction)
        {
            // Check Available space from starting point in direction and return 
            // that space as one-dimensional array with [0] being vertical 
            // and [1] being horizontal space.
            int[] space = CheckSpaceInDirection(wGrid, start, direction);
            if (space[0] >= (int)Size.minRoomSize || space[0] <= (int)Size.maxRoomSize &&
                space[1] >= (int)Size.minRoomSize || space[1] <= (int)Size.maxRoomSize)
            {
                // Generate random Numbers for temporary room array.
                // What if space is smaller than min? 
                // What if max room size is larger than available space? 
                var rand = new Random();
                int randomWidth = rand.Next((int)Size.minRoomSize, space[1] + 1);
                int randomHeight = rand.Next((int)Size.minRoomSize, space[0] + 1);

                // Create temporary room array.
                int[,] room = new int[randomHeight, randomWidth];

                // Shift starting point
                ShiftStartingPoint(start, randomHeight, randomWidth, direction);

                // Draw Room.
                GenerateWalls(randomHeight, randomWidth, room);
                GenerateDoors(randomHeight, randomWidth, room);
            
                // Write Room into center of memGrid.
                PasteToWorkGrid(randomHeight, randomWidth, room, wGrid, start);
            }
            else
            {
                return wGrid;
            }
            return wGrid;
            
        }
        static int[] CheckSpaceInDirection(int[,] wGrid, int[] start, int direction)
        {
            int spaceVert, spaceHori;

            if (direction == (int)Direction.none)
            {
                spaceVert = (int)Size.maxRoomSize;
                spaceHori = (int)Size.maxRoomSize;
            }
            else if (direction == (int)Direction.north)
            {
                spaceVert = CheckAbove(wGrid, start);
                spaceHori = CheckLeft(wGrid, start) + CheckRight(wGrid, start);
            }
            else if (direction == (int)Direction.east)
            {
                spaceVert = CheckAbove(wGrid, start) + CheckBelow(wGrid, start);
                spaceHori = CheckRight(wGrid, start);
            }
            else if (direction == (int)Direction.south)
            {
                spaceVert = CheckBelow(wGrid, start);
                spaceHori = CheckLeft(wGrid, start) + CheckRight(wGrid, start);
            }
            else
            {
                spaceVert = CheckAbove(wGrid, start) + CheckBelow(wGrid, start);
                spaceHori = CheckLeft(wGrid, start);
            }

            int[] space = {spaceVert, spaceHori};
            return space;
        }
        static int CheckAbove(int[,] wGrid, int[] start)
        {
            int c = 0;
            // Start[0] + 1 ??
            for (int i = start[0]; i > 0; i--)
            {
                if (wGrid[i, start[1]] == (int)Tile.blank)
                {
                    c++;
                }
                else
                {
                    break;
                }
            }
            return c;
        }
        static int CheckBelow(int[,] wGrid, int[] start)
        {
            int c = 0;
            for (int i = start[0]; i < (int)Size.canvasH - start[0]; i++)
            {
                if (wGrid[i, start[1]] == (int)Tile.blank)
                {
                    c++;
                }
                else
                {
                    break;
                }
            }
            return c;
        }
        static int CheckLeft(int[,] wGrid, int[] start)
        {
            int c = 0;
            for (int i = start[1]; i > 0; i--)
            {
                if (wGrid[start[0], i] == (int)Tile.blank)
                {
                    c++;
                }
                else
                {
                    break;
                }
            }
            return c;
        }
        static int CheckRight(int[,] wGrid, int[] start)
        {
            int c = 0;
            for (int i = start[1]; i < (int)Size.canvasW - start[1]; i++)
            {
                if (wGrid[start[0], i] == (int)Tile.blank)
                {
                    c++;
                }
                else
                {
                    break;
                }
            }
            return c;
        }
        static int[] ShiftStartingPoint(int[] start, int height, int width, int direction)
        {
            if (direction == (int)Direction.none)
            {
                start[0] = start[0] - (height / 2); 
                start[1] = start[1] - (width / 2);
            }
            else if (direction == (int)Direction.north)
            {
                start[0] = start[0] + height - 1;
                var rand = new Random();
                int shift = rand.Next(1, (width - 2) / 2 + 1);
                start[1] = start[1] + shift;
            }
            else if (direction == (int)Direction.east)
            {
                var rand = new Random();
                int shift = rand.Next(1, (height - 2) / 2 + 1);
                start[0] = start[0] + shift;
            }
            else if (direction == (int)Direction.south)
            {
                var rand = new Random();
                int shift = rand.Next(1, (width - 2) / 2 + 1);
                start[1] = start[1] + shift;
            }
            else if (direction == (int)Direction.west)
            {
                var rand = new Random();
                int shift = rand.Next(1, (height - 2) / 2 + 1);
                start[0] = start[0] + shift;
                start[1] = start[1] + width - 1;
            }
            return start;
        }
        static int[,] GenerateWalls(int height, int width, int[,] room)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Draw northern and southern wall, do not draw over existing doors.
                    if ((i == 0 || i == height - 1) && room[i, j] != (int)Tile.door)
                    {
                        room[i,j] = (int)Tile.wall;
                    }
                    // Draw eastern and western wall, do not draw over existing doors.
                    else if ((j == 0 || j == width - 1) && room[i, j] != (int)Tile.door)
                    {
                        room[i,j] = (int)Tile.wall;
                    }
                    // Draw inside of room.
                    else 
                    {
                        room[i,j] = (int)Tile.floor;
                    }
                }
            }
            return room;
        }
        static int[,] GenerateDoors(int height, int width, int[,]room)
        {
            // Generate random number of doors.
            var rand = new Random();
            int numberOfDoors = rand.Next(1, 5);

            do
            {
                // Go to random wall.
                int pickedWall = rand.Next((int)Direction.north, (int)Direction.west + 1);

                // Check if wall has door.
                if (CheckDoor(pickedWall, height, width, room) == false)
                {
                    // Pick spot on the wall.
                    if (pickedWall == (int)Direction.north)
                    {
                        int pickedSpot = rand.Next(1, width - 2);
                        room[0,pickedSpot] = (int)Tile.door;
                    }
                    else if (pickedWall == (int)Direction.east)
                    {
                        int pickedSpot = rand.Next(1, height - 2);
                        room[pickedSpot, width - 1] = (int)Tile.door;
                    }
                    else if (pickedWall == (int)Direction.south)
                    {
                        int pickedSpot = rand.Next(1, width - 2);
                        room[height - 1, pickedSpot] = (int)Tile.door;
                    }
                    else if (pickedWall == (int)Direction.west)
                    {
                        int pickedSpot = rand.Next(1, height - 2);
                        room[pickedSpot, 0] = (int)Tile.door;
                    }
                }
                numberOfDoors--;
            }
            while ( numberOfDoors > 0);

            return room;
        }
        static bool CheckDoor(int wall, int height, int width, int[,] room)
       {
            if (wall == (int)Direction.north)
            {
                for (int i = 1; i < width - 2; i++)
                {
                    if (room[0, i] == (int)Tile.door)
                    {
                        return true;
                    }
                }
                return false;
            }  
            else if (wall == (int)Direction.east)
            {
                for (int i = 1; i < height - 2; i++)
                {
                    if (room[i, width - 1] == (int)Tile.door)
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (wall == (int)Direction.south)
            {
                for (int i = 1; i < width - 2; i++)
                {
                    if (room[height - 1,i] == (int)Tile.door)
                    {
                        return true;
                    }
                }
                return false;
            }  
            else if (wall == (int)Direction.west)
            {
                for (int i = 1; i < height - 2; i++)
                {
                    if (room[i, 0] == (int)Tile.door)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }     
        } 
        static int[,] PasteToWorkGrid(int height, int width, int[,] room, int[,] wGrid, int [] start)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    wGrid[start[0] + i, start[1] + j] = room[i,j];
                }
            }
            return wGrid;
        }
        static void PrintDungeon(int height, int width, int[,] room)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (room[i,j] == (int)Tile.wall)
                    {
                        Console.Write("#");
                    }
                    else if (room[i,j] == (int)Tile.door)
                    {
                        Console.Write("_");
                    }
                    else if (room[i,j] == (int)Tile.floor)
                    {
                        Console.Write(".");
                    }
                    else if (room[i,j] == (int)Tile.corridor)
                    {
                        Console.Write(".");
                    }
                    else if (room[i,j] == (int)Tile.blank)
                    {
                        Console.Write(" ");
                    }
                }
                Console.Write("\n");
            }
        }
    }
}
