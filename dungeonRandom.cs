using System;
using System.ComponentModel;
using System.Reflection.Metadata;
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
        canvasW = 80, canvasH = 20, maxRoomSize = 12, minRoomSize = 3,
    }
    enum Tile
    {        
        blank, wall, door, floor, corridor,
    }
    struct Point
    {
        public int index;
        public int y;
        public int x;
        public Direction direction;
        public Tile type;
    }

    // Draws a dungeon with a random number of randomly sized rooms 
    // that have a random number of doors, connected by randomly long corridors. 
    class Program
    {
        static void Main(string[] args)
        {
            // Set grid to Blank Space
            Tile[,] grid = new Tile[(int)Size.canvasH, (int)Size.canvasW];
            for (int i = 0; i < (int)Size.canvasH; i++)
            {
                for (int j = 0; j < (int)Size.canvasW; j++)
                {
                    grid[i,j] = Tile.blank;
                }
            }

            // Create starting point and set to middle 
            Point start;
            start.y = (int)Size.canvasH / 2; 
            start.x = (int)Size.canvasW / 2;
            start.direction = Direction.none;
            start.index = 0;
            start.type = Tile.blank;

            // Copy Grid into working Grid
            Tile[,] workGrid = new Tile[(int)Size.canvasH, (int)Size.canvasW];
            CopyGridToWorkGrid(grid, workGrid);

            // Generate a random Room 
            GenerateRoom(workGrid, start);

            // Set end of corridor to type door and set new startingpoint at door.

            // Draw another room around the startingpoints.

            // Print Dungeon.
            PrintDungeon(workGrid);
        }

        // Copies the Base Grid into a working grid.
        static Tile[,] CopyGridToWorkGrid(Tile[,] grid, Tile[,] wGrid)
        {
            for (int i = 0; i < (int)Size.canvasH; i++)
            {
                for (int j = 0; j < (int)Size.canvasW; j++)
                {
                    wGrid[i,j] = grid[i,j];
                }
            }
            return wGrid;
        }

        static Tile[,] GenerateRoom(Tile[,] wGrid, Point start)
        {
            // Check Available space from starting point in direction and return 
            // that space as one-dimensional array with [0] being vertical 
            // and [1] being horizontal space.
            int[] space = CheckSpaceInDirection(wGrid, start);
            if (space[0] >= (int)Size.minRoomSize && space[1] >= (int)Size.minRoomSize)
            {
                // Generate random Numbers for temporary room array.
                // What if space is smaller than min? 
                // What if max room size is larger than available space? 
                var rand = new Random();
                int randomWidth = rand.Next((int)Size.minRoomSize, space[1] + 1);
                int randomHeight = rand.Next((int)Size.minRoomSize, space[0] + 1);

                // Create temporary room array.
                Tile[,] room = new Tile[randomHeight, randomWidth];

                // Shift starting point
                start = ShiftStartingPoint(start, randomHeight, randomWidth);

                // Draw Room.
                GenerateWalls(randomHeight, randomWidth, room, wGrid);

                // TODO
                // Generate Doors needs to somehow store 
                // the direction and spot of every door generated.
                // With that knowledge we can then draw corridor from every door &
                // then update the spot we used to the end of the corridor.
                // Then we can draw a door in that spot and draw a room around that door 
                // like we did with the starting room.
                // Every door should get a specific Point assigned that stores location and direction.
                // Generate random number of doors.
                int numberOfDoors = rand.Next(1, 5);
                Point[] doorInRoom = new Point[numberOfDoors];
                GenerateDoors(randomHeight, randomWidth, room, ref doorInRoom, numberOfDoors, start);

                // Write Room into center of memGrid.
                PasteToWorkGrid(randomHeight, randomWidth, room, wGrid, start);

                // Generate corridors.
                Point[] endOfCorridor = new Point[numberOfDoors];
                GenerateCorridors(wGrid, doorInRoom, ref endOfCorridor, numberOfDoors);
            }
            else
            {
                return wGrid;
            }
            return wGrid;
            
        }

        // Checks against direction input and starting point.
        // Then measures the distance from that spot in x and y direction,
        // until it finds a space that is not blank. 
        // Stores space in x and y direction as array with 2 values.
        // If direction is none (start of program), space is set to maximum room size.
        static int[] CheckSpaceInDirection(Tile[,] wGrid, Point start)
        {
            int spaceVert = 0, spaceHori = 0;
            switch (start.direction)
            {
                case Direction.none:
                    spaceVert = (int)Size.maxRoomSize;
                    spaceHori = (int)Size.maxRoomSize;
                    break;
                case Direction.north:
                    spaceVert = CheckAbove(wGrid, start);
                    spaceHori = CheckLeft(wGrid, start) + CheckRight(wGrid, start);
                    break;
                case Direction.east:
                    spaceVert = CheckAbove(wGrid, start) + CheckBelow(wGrid, start);
                    spaceHori = CheckRight(wGrid, start);
                    break;
                case Direction.south:
                    spaceVert = CheckBelow(wGrid, start);
                    spaceHori = CheckLeft(wGrid, start) + CheckRight(wGrid, start);
                    break;
                case Direction.west:
                    spaceVert = CheckAbove(wGrid, start) + CheckBelow(wGrid, start);
                    spaceHori = CheckLeft(wGrid, start);
                    break;
            }

            int[] space = [spaceVert, spaceHori];
            return space;
        }

        // Measures distance to non blank space above starting point 
        // going towards the upper edge of the base grid.
        static int CheckAbove(Tile[,] wGrid, Point start)
        {
            int c = 0;
            // Start[0] + 1 ??
            for (int i = start.y; i > 0; i--)
            {
                switch (wGrid[i, start.x])
                {
                    case Tile.blank:
                        c++;
                        break;
                    default: 
                        return c;
                }
            }
            return c;
        }

        // Measures distance to non blank space below starting point 
        // going towards the lower edge of the base grid.
        static int CheckBelow(Tile[,] wGrid, Point start)
        {
            int c = 0;
            for (int i = start.y; i < (int)Size.canvasH - start.y; i++)
            {
                if (wGrid[i, start.x] == (int)Tile.blank)
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

        // Measures distance to non blank space left of starting point 
        // going towards the left edge of the base grid.
        static int CheckLeft(Tile[,] wGrid, Point start)
        {
            int c = 0;
            for (int i = start.x; i > 0; i--)
            {
                if (wGrid[start.y, i] == (int)Tile.blank)
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

        // Measures distance to non blank space right of starting point 
        // going towards the right edge of the base grid.
        static int CheckRight(Tile[,] wGrid, Point start)
        {
            int c = 0;
            for (int i = start.x; i < (int)Size.canvasW - start.x; i++)
            {
                if (wGrid[start.y, i] == (int)Tile.blank)
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

        // Checks from which direction the starting point for drawing a room was reached
        // Then shifts the starting point by a random number, based on previously calculated
        // room size.
        static Point ShiftStartingPoint(Point start, int height, int width)
        {
            var rand = new Random();
            int shift;
            switch (start.direction)
            {
                case Direction.none:
                    start.y = start.y - (height / 2); 
                    start.x = start.x - (width / 2);
                    return start;
                case Direction.north:
                    start.y = start.y + height - 1;
                    shift = rand.Next(1, (width - 2) / 2 + 1);
                    start.x = start.x + shift;
                    return start;
                case Direction.east:
                    shift = rand.Next(1, (height - 2) / 2 + 1);
                    start.y = start.y + shift;
                    return start;
                case Direction.south:
                    shift = rand.Next(1, (width - 2) / 2 + 1);
                    start.x = start.x + shift;
                    return start;
                case Direction.west:
                    shift = rand.Next(1, (height - 2) / 2 + 1);
                    start.y = start.y + shift;
                    start.x = start.x + width - 1;
                    return start;
            }
            return start;
        }

        // Draws a wall around the room. (Edge aligned INSIDE the room)
        // If there is a door, no wall is drawn in that space. (Doors stay doors)
        static Tile[,] GenerateWalls(int height, int width, Tile[,] room, Tile[,] wGrid)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Draw northern and southern wall, do not draw over existing doors.
                    if ((i == 0 || i == height - 1) && wGrid[i, j] != Tile.door)
                    {
                        room[i,j] = Tile.wall;
                    }
                    // Draw eastern and western wall, do not draw over existing doors.
                    else if ((j == 0 || j == width - 1) && wGrid[i, j] != Tile.door)
                    {
                        room[i,j] = Tile.wall;
                    }
                    // Draw inside of room.
                    else 
                    {
                        room[i,j] = Tile.floor;
                    }
                }
            }
            return room;
        }

        // Draws a door on a randomly picked wall, for a random number of walls.
        // (Only one door per wall).
        // If there is already a door on that wall (CheckDoor[] == false), 
        // no new door is drawn on that wall.
        static Tile[,] GenerateDoors(int height, int width, Tile[,]room, ref Point[] door, int numberOfDoors, Point start)
        {
            do
            {
                door[numberOfDoors - 1].index = numberOfDoors;
                // Go to random wall.
                Array pickedWall = Enum.GetValues(typeof(Direction));
                Random rand = new Random();
                Direction randomWall = (Direction)pickedWall.GetValue(rand.Next(1, pickedWall.Length))!;

                int pickedSpot;

                // Check if wall has door.
                if (CheckDoor(randomWall, height, width, room) == false)
                {
                    // Pick spot on wall
                    switch (randomWall)
                    {
                        case Direction.north:
                            pickedSpot = rand.Next(1, width - 2);
                            room[0,pickedSpot] = Tile.door;
                            door[numberOfDoors - 1].type = Tile.door;
                            door[numberOfDoors - 1].direction = Direction.north;
                            door[numberOfDoors - 1].x = pickedSpot + start.x;
                            door[numberOfDoors - 1].y = 0 + start.y;
                            break;
                        case Direction.east:
                            pickedSpot = rand.Next(1, height - 2);
                            room[pickedSpot, width - 1] = Tile.door;
                            door[numberOfDoors - 1].type = Tile.door;
                            door[numberOfDoors - 1].direction = Direction.east;
                            door[numberOfDoors - 1].x = width - 1 + start.x;
                            door[numberOfDoors - 1].y = pickedSpot + start.y;
                            break;
                        case Direction.south:
                            pickedSpot = rand.Next(1, width - 2);
                            room[height - 1, pickedSpot] = Tile.door;
                            door[numberOfDoors - 1].type = Tile.door;
                            door[numberOfDoors - 1].direction = Direction.south;
                            door[numberOfDoors - 1].x = pickedSpot + start.x;
                            door[numberOfDoors - 1].y = height - 1 + start.y;
                            break;
                        case Direction.west:
                            pickedSpot = rand.Next(1, height - 2);
                            room[pickedSpot, 0] = Tile.door;
                            door[numberOfDoors - 1].type = Tile.door;
                            door[numberOfDoors - 1].direction = Direction.west;
                            door[numberOfDoors - 1].x = 0 + start.x;
                            door[numberOfDoors - 1].y = pickedSpot + start.y;
                            break;
                    }
                }
                numberOfDoors--;
            }
            while (numberOfDoors > 0);

            return room;
        }

        // Generates a corridor in front of each door.
        static Tile[,] GenerateCorridors(Tile[,] wGrid, Point[] door, 
        ref Point[] endOfCorridor, int numberOfDoors)
        {
            for (int i = numberOfDoors - 1; i >= 0; i--)
            {                
                var rand = new Random();
                int length = rand.Next(1, 4);
                switch (door[i].direction)
                {
                    case Direction.north:
                        for (int j = 0; j < length; j++)
                        {
                            wGrid[door[i].y - 1 - j, door[i].x] = Tile.corridor;
                        }
                        break;       
                    case Direction.east:
                        for (int j = 0; j < length; j++)
                        {
                            wGrid[door[i].y, door[i].x + 1 + j] = Tile.corridor;
                        }
                        break;
                    case Direction.south:
                        for (int j = 0; j < length; j++)
                        {
                            wGrid[door[i].y + 1 + j, door[i].x] = Tile.corridor;
                        }
                        break;
                    case Direction.west:
                        for (int j = 0; j < length; j++)
                        {
                            wGrid[door[i].y, door[i].x - 1 - j] = Tile.corridor;
                        }
                        break;
                }
            }
            return wGrid;
        }

        // Checks if a wall in a given direction has a door or not.
        static bool CheckDoor(Direction wall, int height, int width, Tile[,] room)
       {
            switch (wall)
            {
                case Direction.north:
                    for (int i = 1; i < width - 2; i++)
                    {
                        if (room[0, i] == Tile.door)
                        {
                            return true;
                        }
                    }
                    return false;
                case Direction.east:
                    for (int i = 1; i < height - 2; i++)
                    {
                        if (room[i, width - 1] == Tile.door)
                        {
                            return true;
                        }
                    }
                    return false;
                case Direction.south:
                    for (int i = 1; i < width - 2; i++)
                    {
                        if (room[height - 1,i] == Tile.door)
                        {
                            return true;
                        }
                    }
                    return false;
                case Direction.west:
                    for (int i = 1; i < height - 2; i++)
                    {
                        if (room[i, 0] == Tile.door)
                        {
                            return true;
                        }
                    }
                    return false;
                default:
                    return false;
            }
        } 

        // Pastes the room into the working Grid.
        static Tile[,] PasteToWorkGrid(int height, int width, Tile[,] room, Tile[,] wGrid, Point start)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    wGrid[start.y + i, start.x + j] = room[i,j];
                }
            }
            return wGrid;
        }

        // Prints the Dungeon. (Currently using the working grid)
        // Currently prints floor as '.' walls as '#' doors as '_' and corridors as '.'
        // Blank spaces are left blank.
        static void PrintDungeon(Tile[,] grid)
        {
            for (int i = 0; i < (int)Size.canvasH; i++)
            {
                for (int j = 0; j < (int)Size.canvasW; j++)
                {
                    switch (grid[i, j])
                    {
                        case Tile.wall:
                            Console.Write("#");
                            break;
                        case Tile.door:
                            Console.Write("_");
                            break;
                        case Tile.floor:
                            Console.Write(".");
                            break;
                        case Tile.corridor:
                            Console.Write(".");
                            break;
                        case Tile.blank:
                            Console.Write(" ");
                            break;  
                    }
                }
                Console.Write("\n");
            }
        }
    }
}
