using System;
using System.ComponentModel;

// Possible Values a space on grid can have
// Wall, Door, Floor, Corridor, Blank
// Wall = 1, Door = 2, Floor = 3, Corridor = 4, Blank = 0;

namespace DungeonRandom
{
    static class Globals
    {
        public static int canvasWidth = 80;
        public static int canvasHeight = 20;
        public static int max = 10;
        public static int min = 3;
        
        // Floor tiles
        public static int wall = 1;
        public static int door = 2;
        public static int floor = 3;
        public static int corridor = 4;
        public static int blank = 0;

        // Directions
        public static int north = 1;
        public static int east = 2;
        public static int south = 3;
        public static int west = 4;
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Set grid to Blank Space
            int[,] grid = new int[Globals.canvasHeight, Globals.canvasWidth];
            for (int i = 0; i < Globals.canvasHeight; i++)
            {
                for (int j = 0; j < Globals.canvasWidth; j++)
                {
                    grid[i,j] = Globals.blank;
                    // Console.Write("0");
                }
                // Console.Write("\n");
            }

            // Generate a random Room 
            generateRoom(Globals.max, Globals.min);
        }
        static void generateRoom(int max, int min)
        {
            int[,] memGrid = new int[Globals.canvasHeight, Globals.canvasWidth];
            // Generate random Numbers for temporary room array
            var rand = new Random();
            int randomWidth = rand.Next(min, max) ;
            int randomHeight = rand.Next(min, max);

            // Create temporary room array
            int[,] room = new int[randomHeight, randomWidth];

            // Draw Walls
            generateWalls(randomHeight, randomWidth, room);

            generateDoors(randomHeight, randomWidth, room);

            // Write Room into center of memGrid
            for (int i = 0; i < randomHeight; i++)
            {
                for (int j = 0; j < randomWidth; j++)
                {
                    // RUNDEN wäre gut hier
                    memGrid[(Globals.canvasHeight / 2) - (randomHeight / 2) + i, (Globals.canvasWidth / 2) - (randomHeight / 2) + j] = room[i,j];
                }
            }
            // DEBUG Output of random room
            printDungeon(Globals.canvasHeight, Globals.canvasWidth, memGrid);
        }
        static int[,] generateWalls(int height, int width, int[,] room)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Draw northern and southern wall
                    if (i == 0 || i == height - 1)
                    {
                        room[i,j] = Globals.wall;
                    }
                    // Draw eastern and western wall
                    else if (j == 0 || j == width - 1)
                    {
                        room[i,j] = Globals.wall;
                    }
                    // Draw inside of room
                    else 
                    {
                        room[i,j] = Globals.floor;
                    }
                }
            }
            return room;
        }
        static int[,] generateDoors(int height, int width, int[,]room)
        {
            // Go to random wall
            // North = 1, East = 2, South = 3, West = 4
            var rand = new Random();
            int pickedWall = rand.Next(Globals.north, Globals.west + 1);

            // Check if wall has door
            if (checkDoor(pickedWall, height, width, room) == false)
            {
                // Pick spot on the wall
                if (pickedWall == Globals.north)
                {
                    int pickedSpot = rand.Next(1, width - 2);
                    room[0,pickedSpot] = Globals.door;
                }
                else if (pickedWall == Globals.east)
                {
                    int pickedSpot = rand.Next(1, height - 2);
                    room[pickedSpot, width - 1] = Globals.door;
                }
                else if (pickedWall == Globals.south)
                {
                    int pickedSpot = rand.Next(1, width - 2);
                    room[height - 1, pickedSpot] = Globals.door;
                }
                else if (pickedWall == Globals.west)
                {
                    int pickedSpot = rand.Next(1, height - 2);
                    room[pickedSpot, 0] = Globals.door;
                }
            }
            return room;
        }
        static bool checkDoor(int wall, int height, int width, int[,] room)
       {
            if (wall == Globals.north)
            {
                for (int i = 1; i < width - 2; i++)
                {
                    if (room[0, i] == Globals.door)
                    {
                        return true;
                    }
                }
                return false;
            }  
            else if (wall == Globals.east)
            {
                for (int i = 1; i < height - 2; i++)
                {
                    if (room[i, width - 1] == Globals.door)
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (wall == Globals.south)
            {
                for (int i = 1; i < width - 2; i++)
                {
                    if (room[height - 1,i] == Globals.door)
                    {
                        return true;
                    }
                }
                return false;
            }  
            else if (wall == Globals.west)
            {
                for (int i = 1; i < height - 2; i++)
                {
                    if (room[i, 0] == Globals.door)
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
        static void printDungeon(int height, int width, int[,] room)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (room[i,j] == Globals.wall)
                    {
                        Console.Write("#");
                    }
                    else if (room[i,j] == Globals.door)
                    {
                        Console.Write(".");
                    }
                    else if (room[i,j] == Globals.floor)
                    {
                        Console.Write(".");
                    }
                    else if (room[i,j] == Globals.corridor)
                    {
                        Console.Write(".");
                    }
                    else if (room[i,j] == Globals.blank)
                    {
                        Console.Write(" ");
                    }
                }
                Console.Write("\n");
            }
        }
    }
}
