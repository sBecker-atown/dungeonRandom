using System;
using System.ComponentModel;
using System.Data;
using System.Reflection.Metadata;
using System.Xml;

namespace DungeonRandom
{
    enum Direction
    {
        none, north, east, south, west,
    }
    class Size
    {
        // Canvas Sizes should be multiplicants of 2 
        public const int canvasW = 80; 
        public const int canvasH = 20; 
        public const int maxRoomSize = 10; 
        public const int minRoomSize = 3; 
        public const int maxCorridorLength = 3;
    }
    enum Tile
    {        
        blank, wall, door, floor, corridor,
    }
    struct Point
    {
        public int y;
        public int x;
        public Direction direction;
        public Tile type;
    }

    class Door
    {
        public Direction direction;
        public int y;
        public int x;
        public bool used;
    }
 
    // Draws a dungeon with a random number of randomly sized rooms 
    // that have a random number of doors, connected by randomly long corridors. 
    class Program
    {
        static void Main(string[] args)
        {
            // Set grid to Blank Space
            Tile[,] grid = new Tile[Size.canvasH, Size.canvasW];
            for (int i = 0; i < Size.canvasH; i++)
            {
                for (int j = 0; j < Size.canvasW; j++)
                {
                    grid[i,j] = Tile.blank;
                }
            }

            // Create starting point and set to middle 
            Point start;
            start.y = Size.canvasH / 2; 
            start.x = Size.canvasW / 2;
            start.direction = Direction.none;
            start.type = Tile.blank;

            // Copy Grid into working Grid
            Tile[,] workGrid = new Tile[Size.canvasH, Size.canvasW];
            CopyGridToWorkGrid(grid, workGrid);

            List<Door> allDoors = new List<Door>();
            // Generate a random Room 
            GenerateRoom(workGrid, start, allDoors);

            // Draw another room around the startingpoints.
            /*for (int i = 0; i < allDoors.Count; i++)
            {
               if (allDoors[i].used == false)
               {
                   start.y = allDoors[i].y;
                   start.x = allDoors[i].x;
                   start.direction = allDoors[i].direction;
                   GenerateRoom(workGrid, start, allDoors);
                   allDoors[i].used = true;
               }
            } */

            // Print Dungeon.
            PrintDungeon(workGrid);
        }

        // Copies the Base Grid into a working grid.
        static Tile[,] CopyGridToWorkGrid(Tile[,] grid, Tile[,] wGrid)
        {
            for (int i = 0; i < Size.canvasH; i++)
            {
                for (int j = 0; j < Size.canvasW; j++)
                {
                    wGrid[i,j] = grid[i,j];
                }
            }
            return wGrid;
        }

        static Tile[,] GenerateRoom(Tile[,] wGrid, Point start, List<Door> allDoors)
        {
            // Check Available space from starting point in direction and return 
            // that space as one-dimensional array with [0] being vertical 
            // and [1] being horizontal space.
            int[] space = CheckSpaceInDirection(wGrid, start);
            if (space[0] >= Size.minRoomSize && space[1] >= Size.minRoomSize)
            {
                var rand = new Random();
                int randW = rand.Next(Size.minRoomSize, space[1] + 1);
                if ((int)Size.minRoomSize == space[1]) randW = 3;
                if (space[1] > Size.maxRoomSize) randW = Size.maxRoomSize;
                int randH = rand.Next(Size.minRoomSize, space[0] + 1);
                if ((int)Size.minRoomSize == space[0]) randH = 3;
                if (space[0] > Size.maxRoomSize) randH = Size.maxRoomSize;

                // Create temporary room array.
                Tile[,] room = new Tile[randH, randW];

                // Shift starting point
                start = ShiftStartingPoint(start, randH, randW);

                // Draw Room.
                GenerateWalls(randH, randW, room, wGrid);

                // Get random number of doors (max 1 door per wall)
                int numberOfDoors = rand.Next(1, 5);

                // Draw Doors on randomly picked wall
                GenerateDoors(randH, randW, room, 
                numberOfDoors, start, allDoors);

                // Write Room into center of memGrid.
                PasteToWorkGrid(randH, randW, room, wGrid, start);

                // Generate corridors.
                GenerateCorridors(wGrid, allDoors, start);
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
                    spaceVert = Size.maxRoomSize;
                    spaceHori = Size.maxRoomSize;
                    break;
                case Direction.north:
                    // After checking Space above, steps out of room one space, in order to not
                    // collide with rooms own wall when counting left and right. (start.y = start.y - 1)
                    // Afterwards, steps back to original starting position (start.y = start.y + 1)
                    spaceVert = CheckAbove(wGrid, start);
                    start.y = start.y - 1;
                    spaceHori = CheckLeft(wGrid, start) + CheckRight(wGrid, start) + 1;
                    start.y = start.y + 1;
                    break;
                case Direction.east:
                    spaceHori = CheckRight(wGrid, start);
                    start.x = start.x + 1;
                    spaceVert = CheckAbove(wGrid, start) + CheckBelow(wGrid, start) + 1;
                    start.x = start.x - 1;
                    break;
                case Direction.south:
                    spaceVert = CheckBelow(wGrid, start);
                    start.y = start.y + 1;
                    spaceHori = CheckLeft(wGrid, start) + CheckRight(wGrid, start) + 1;
                    start.y = start.y - 1;
                    break;
                case Direction.west:
                    spaceHori = CheckLeft(wGrid, start);
                    start.x = start.x - 1;
                    spaceVert = CheckAbove(wGrid, start) + CheckBelow(wGrid, start) + 1;
                    start.x = start.x + 1;
                    break;
            }

            int[] spaceInDirection = [spaceVert, spaceHori];
            return spaceInDirection;
        }

        // Measures distance to non blank space above starting point 
        // going towards the upper edge of the base grid.
        static int CheckAbove(Tile[,] wGrid, Point start)
        {
            int c = 0;
            for (int i = start.y - 1; i >= 0; i--)
            {
                if (wGrid[i, start.x] == Tile.blank)
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

        // Measures distance to non blank space below starting point 
        // going towards the lower edge of the base grid.
        static int CheckBelow(Tile[,] wGrid, Point start)
        {
            int c = 0;
            for (int i = start.y + 1; i < Size.canvasH; i++)
            {
                if (wGrid[i, start.x] == Tile.blank)
                {
                    c++;
                }
                else{
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
            for (int i = start.x - 1; i >= 0; i--)
            {
                if (wGrid[start.y, i] == Tile.blank)
                {
                    c++;
                }
                else{
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
            for (int i = start.x + 1; i < Size.canvasW; i++)
            {
                if (wGrid[start.y, i] == Tile.blank)
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
        static Tile[,] GenerateDoors(int height, int width, Tile[,]room, 
        int numberOfDoors, Point start, List<Door> allDoors)
        {
            do
            {
                // Go to random wall.
                Array pickedWall = Enum.GetValues(typeof(Direction));
                Random rand = new Random();
                Direction randomWall = (Direction)pickedWall.GetValue(rand.Next(1, pickedWall.Length))!;

                int pickedSpot;
                Door thisDoor = new Door();

                // Check if wall has door.
                if (CheckDoor(randomWall, height, width, room) == false)
                {
                    // Pick spot on wall
                    switch (randomWall)
                    {
                        case Direction.north:
                            pickedSpot = rand.Next(1, width - 2);
                            room[0,pickedSpot] = Tile.door;

                            thisDoor.direction = Direction.north;
                            thisDoor.x = pickedSpot + start.x;
                            thisDoor.y = 0 + start.y;
                            thisDoor.used = false;

                            allDoors.Add(thisDoor);
                            break;

                        case Direction.east:
                            pickedSpot = rand.Next(1, height - 2);
                            room[pickedSpot, width - 1] = Tile.door;

                            thisDoor.direction = Direction.east;
                            thisDoor.x = width - 1 + start.x;
                            thisDoor.y = pickedSpot + start.y;
                            thisDoor.used = false;

                            allDoors.Add(thisDoor);
                            break;
                            
                        case Direction.south:
                            pickedSpot = rand.Next(1, width - 2);
                            room[height - 1, pickedSpot] = Tile.door;
                            thisDoor.direction = Direction.south;
                            thisDoor.x = pickedSpot + start.x;
                            thisDoor.y = height - 1 + start.y;
                            thisDoor.used = false;

                            allDoors.Add(thisDoor);
                            break;                         
                            
                        case Direction.west:
                            pickedSpot = rand.Next(1, height - 2);
                            room[pickedSpot, 0] = Tile.door;

                            thisDoor.direction = Direction.west; 
                            thisDoor.x = 0 + start.x;
                            thisDoor.y = pickedSpot + start.y;
                            thisDoor.used = false;

                            allDoors.Add(thisDoor);
                            break;                            
                    }
                }
                numberOfDoors--;
            }
            while (numberOfDoors > 0);

            return room;
        }

        // Generates a corridor in front of each door.
        static Tile[,] GenerateCorridors(Tile[,] wGrid, List<Door> allDoors, Point start)
        {
            // Get number of doors
            int count = 0;
            foreach (Door thisDoor in allDoors)
            {
                if (thisDoor.used == false)
                {
                    count++;
                }
            }

            for (int i = count - 1; i >= 0; i--)
            {                
                start.x = allDoors[i].x;
                start.y = allDoors[i].y;
                start.direction = allDoors[i].direction;
                int[] space = CheckSpaceInDirection(wGrid, start);
                var rand = new Random();
                int length = rand.Next(1, 4);
                switch (allDoors[i].direction)
                {
                    case Direction.north:
                        if (space[0] == 0) break;
                        else if (space[0] < Size.maxCorridorLength) length = rand.Next(1, space[0] + 1);
                        for (int j = 0; j < length; j++)
                        {
                            allDoors[i].used = true;
                            wGrid[allDoors[i].y - 1 - j, allDoors[i].x] = Tile.corridor;
                            if (j == length - 1)
                            {
                                wGrid[allDoors[i].y - 1 - j, allDoors[i].x] = Tile.door;
                                Door thisDoor = new Door
                                {
                                    direction = Direction.north,
                                    x = allDoors[i].x,
                                    y = allDoors[i].y - 1 - j,
                                    used = false
                                };
                                allDoors.Add(thisDoor);
                            }
                        }
                        break;       
                    case Direction.east:
                        if (space[1] == 0) break;
                        else if (space[1] < Size.maxCorridorLength) length = rand.Next(1, space[1] + 1);
                        for (int j = 0; j < length; j++)
                        {
                            allDoors[i].used = true;
                            wGrid[allDoors[i].y, allDoors[i].x + 1 + j] = Tile.corridor;
                            if (j == length - 1)
                            {
                                wGrid[allDoors[i].y, allDoors[i].x + 1 + j] = Tile.door;
                                Door thisDoor = new Door
                                {
                                    direction = Direction.east,
                                    x = allDoors[i].x + 1 + j,
                                    y = allDoors[i].y,
                                    used = false
                                };
                                allDoors.Add(thisDoor);
                            }
                        }
                        break;
                    case Direction.south:
                        if (space[0] == 0) break;
                        else if (space[0] < Size.maxCorridorLength) length = rand.Next(1, space[0] + 1);
                        for (int j = 0; j < length; j++)
                        {
                            allDoors[i].used = true;
                            wGrid[allDoors[i].y + 1 + j, allDoors[i].x] = Tile.corridor;
                            if (j == length - 1)
                            {
                                wGrid[allDoors[i].y + 1 + j, allDoors[i].x] = Tile.door;
                                Door thisDoor = new Door
                                {
                                    direction = Direction.south,
                                    x = allDoors[i].x,
                                    y = allDoors[i].y + 1 + j,
                                    used = false
                                };
                                allDoors.Add(thisDoor);
                            }
                        }
                        break;
                    case Direction.west:
                        if (space[1] == 0) break;
                        else if (space[1] < Size.maxCorridorLength) length = rand.Next(1, space[1] + 1);
                        for (int j = 0; j < length; j++)
                        {
                            allDoors[i].used = true;
                            wGrid[allDoors[i].y, allDoors[i].x - 1 - j] = Tile.corridor;
                            if (j == length - 1)
                            {
                                wGrid[allDoors[i].y, allDoors[i].x - 1 - j] = Tile.door;
                                Door thisDoor = new Door
                                {
                                    direction = Direction.west,
                                    x = allDoors[i].x - 1 - j,
                                    y = allDoors[i].y,
                                    used = false
                                };
                                allDoors.Add(thisDoor);
                            }
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
            for (int i = 0; i < Size.canvasH; i++)
            {
                for (int j = 0; j < Size.canvasW; j++)
                {
                    switch (grid[i, j])
                    {
                        case Tile.wall:
                            Console.Write("#");
                            break;
                        case Tile.door:
                            Console.Write("/");
                            break;
                        case Tile.floor:
                            Console.Write(" ");
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
