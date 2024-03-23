using System;
using System.ComponentModel;

namespace DungeonRandom
{
    enum Direction
    {
        north,
        east,
        south,
        west,
    }
    enum Size
    {
        canvasW = 80,
        canvasH = 20,
        maxRoomSize = 10,
        minRoomSize = 3,
    }
    enum Tile
    {        
        blank,
        wall,
        door,
        floor,
        corridor,
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Set grid to Blank Space
            int[,] grid = new int[(int)Size.canvasH, (int)Size.canvasW];
            for (int i = 0; i < (int)Size.canvasH; i++)
            {
                for (int j = 0; j < (int)Size.canvasW; j++)
                {
                    grid[i,j] = (int)Tile.blank;
                }
            }

            // Generate a random Room 
            GenerateRoom((int)Size.maxRoomSize, (int)Size.minRoomSize);
        }
        static void GenerateRoom(int max, int min)
        {
            int[,] memGrid = new int[(int)Size.canvasH, (int)Size.canvasW];
            // Generate random Numbers for temporary room array.
            var rand = new Random();
            int randomWidth = rand.Next((int)Size.minRoomSize, (int)Size.maxRoomSize) ;
            int randomHeight = rand.Next((int)Size.minRoomSize, (int)Size.maxRoomSize);

            // Create temporary room array.
            int[,] room = new int[randomHeight, randomWidth];

            // Draw Walls.
            GenerateWalls(randomHeight, randomWidth, room);

            GenerateDoors(randomHeight, randomWidth, room);

            // Write Room into center of memGrid.
            for (int i = 0; i < randomHeight; i++)
            {
                for (int j = 0; j < randomWidth; j++)
                {
                    // RUNDEN wäre gut hier.
                    memGrid[((int)Size.canvasH / 2) - (randomHeight / 2) + i, ((int)Size.canvasW / 2) - (randomHeight / 2) + j] = room[i,j];
                }
            }
            // DEBUG Output of random room.
            PrintDungeon((int)Size.canvasH, (int)Size.canvasW, memGrid);
        }
        static int[,] GenerateWalls(int height, int width, int[,] room)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Draw northern and southern wall.
                    if (i == 0 || i == height - 1)
                    {
                        room[i,j] = (int)Tile.wall;
                    }
                    // Draw eastern and western wall.
                    else if (j == 0 || j == width - 1)
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
            // Go to random wall.
            // North = 1, East = 2, South = 3, West = 4.
            var rand = new Random();
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
                        Console.Write(".");
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
