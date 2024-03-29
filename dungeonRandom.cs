using System;
using System.ComponentModel;
using System.Data;
using System.Reflection.Metadata;
using System.Xml;

namespace DungeonRandom
{
    class Size
    {
        // Canvas Sizes should be multiplicants of 2 
        public const int canvasW = 80; 
        public const int canvasH = 20; 
        public const int maxRoomSize = 10; 
        public const int minRoomSize = 3; 
        public const int maxCorridorLength = 5;
        public const int minCorridorLength = 2;

    }
    enum Direction
    {
        none, north, east, south, west,
    }
    enum DoorState
    {
        opened, unopened, corridorEnd,
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
        public DoorState status;
    }
 
    // Draws a dungeon with a random number of randomly sized 
    // rooms that have a random number of doors, connected by 
    // randomly long corridors. 
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
            // start.x = 0;
            // start.y = 0;
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
            // Generate corridors.
            GenerateCorridors(workGrid, allDoors, start);

            // Draw another room around the doors at 
            // the end corridors
            for (int i = 0; i < allDoors.Count; i++)
            {
               if (allDoors[i].status == DoorState.corridorEnd)
               {
                   start.y = allDoors[i].y;
                   start.x = allDoors[i].x;
                   start.direction = allDoors[i].direction;
                   GenerateRoom(workGrid, start, allDoors);
                   allDoors[i].status = DoorState.opened;
                   GenerateCorridors(workGrid, allDoors, start);
               }
            }

            // Print Dungeon.
            PrintDungeon(workGrid);
        }

        // Copies the Base Grid into a working grid.
        static Tile[,] CopyGridToWorkGrid(Tile[,] grid, 
        Tile[,] wGrid)
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

        static Tile[,] GenerateRoom(Tile[,] wGrid, Point start, 
        List<Door> allDoors)
        {
            // Check Available space from starting point in 
            // direction and return that space as one-dimensional
            // array with [0] being vertical and [1] being 
            // horizontal space.
            int[] space = CheckSpaceInDirection(wGrid, start);
            if (space[0] >= Size.minRoomSize && 
            space[1] >= Size.minRoomSize && 
            SpaceToShift(wGrid, start))
            {
                int randW = SetRoomDimension(space[1]);
                int randH = SetRoomDimension(space[0]);

                // Create temporary room array.
                Tile[,] room = new Tile[randH, randW];

                // Shift starting point
                start = ShiftStartingPoint(start, randH, randW, wGrid);                

                // Draw Room.
                GenerateWalls(randH, randW, room, wGrid, start);

                // Get random number of doors (max 1 door per wall)
                var rand = new Random();
                int numberOfDoors = rand.Next(1, 5);

                // Draw Doors on randomly picked wall
                GenerateDoors(randH, randW, room, 
                numberOfDoors, start, allDoors);

                // Write Room into center of memGrid.
                PasteToWorkGrid(randH, randW, room, wGrid, start);
            }
            else
            {
                return wGrid;
            }
            return wGrid;
            
        }

        // Checks against direction input and starting point.
        // Then measures the distance from that spot in x and 
        // y direction, until it finds a space that is not blank.
        // Stores space in x and y direction as array with 
        // 2 values. If direction is none (start of program), 
        // space is set to maximum room size.
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
        // After checking Space above, steps out of 
        // room one space, in order to not collide 
        // with rooms own wall when counting left and
        // right. (start.y = start.y - 1)
        // Afterwards, steps back to original starting
        // position (start.y = start.y + 1)

                    spaceVert = CheckAbove(wGrid, start);
                    
                    start.y = start.y - 1;
                    spaceHori = CheckLeft(wGrid, start) + 
                    CheckRight(wGrid, start) + 1;
                    start.y = start.y + 1;
                    break;

                case Direction.east:
                    spaceHori = CheckRight(wGrid, start);
                    
                    start.x = start.x + 1;
                    spaceVert = CheckAbove(wGrid, start) + 
                    CheckBelow(wGrid, start) + 1;
                    start.x = start.x - 1;
                    break;

                case Direction.south:
                    spaceVert = CheckBelow(wGrid, start);
                   
                    start.y = start.y + 1;
                    spaceHori = CheckLeft(wGrid, start) + 
                    CheckRight(wGrid, start) + 1;
                    start.y = start.y - 1;
                    break;

                case Direction.west:
                    spaceHori = CheckLeft(wGrid, start);
                    
                    start.x = start.x - 1;
                    spaceVert = CheckAbove(wGrid, start) + 
                    CheckBelow(wGrid, start) + 1;
                    start.x = start.x + 1;
                    break;
            }

            int[] spaceInDirection = [spaceVert, spaceHori];
            return spaceInDirection;
        }

        static bool SpaceToShift(Tile[,] wGrid, Point start)
        {
            switch (start.direction)
            {
                case Direction.none:
                    return true;
                case Direction.north:
                    if (CheckLeft(wGrid, start) >= 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.east:
                    if (CheckAbove(wGrid, start) >= 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.south:
                    if (CheckLeft(wGrid, start) >= 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.west:
                    if (CheckLeft(wGrid, start) >= 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default: 
                return false;
            }
        }

        // Measures distance to non blank space above starting 
        // point going towards the upper edge of the base grid.
        static int CheckAbove(Tile[,] wGrid, Point start)
        {
            int c = 0;
            for (int i = start.y - 1; i >= 0; i--)
            {
                if (!OutOfBound(i, start.x) && wGrid[i, start.x] == Tile.blank)
                {
                        c++;
                }
                else
                {
                    return c;
                }
            }
            return c;
        }

        // Measures distance to non blank space below starting 
        // point going towards the lower edge of the base grid.
        static int CheckBelow(Tile[,] wGrid, Point start)
        {
            int c = 0;
            for (int i = start.y + 1; i < Size.canvasH; i++)
            {
                if (!OutOfBound(i, start.x) && wGrid[i, start.x] == Tile.blank)
                {
                    c++;
                }
                else{
                    return c;
                }
            }
            return c;
        }

        // Measures distance to non blank space left of starting 
        // point going towards the left edge of the base grid.
        static int CheckLeft(Tile[,] wGrid, Point start)
        {
            int c = 0;
            for (int i = start.x - 1; i >= 0; i--)
            {
                if (!OutOfBound(start.y, i) && wGrid[start.y, i] == Tile.blank)
                {
                    c++;
                }
                else{
                    return c;
                }
            }
            return c;
        }

        // Measures distance to non blank space right of starting 
        // point going towards the right edge of the base grid.
        static int CheckRight(Tile[,] wGrid, Point start)
        {
            int c = 0;
            for (int i = start.x + 1; i < Size.canvasW; i++)
            {
                if (!OutOfBound(start.y, i) && wGrid[start.y, i] == Tile.blank)
                {
                    c++;
                }
                else{
                    return c;
                }
            }
            return c;
        }

        static int SetRoomDimension(int space)
        {
            var rand = new Random();
            int size = rand.Next(Size.minRoomSize, space + 1);
            if (Size.minRoomSize == space)
            {
                size = 3;
            }
            else if (space > Size.maxRoomSize) 
            {
                size = Size.maxRoomSize;
            }
            return size;
        }

        // Checks from which direction the starting point for 
        // drawing a room was reached. Then shifts the starting 
        // point by a random number, based on previously 
        // calculated room size.
        static Point ShiftStartingPoint(Point start, int height, int width, Tile[,] wGrid)
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
                    start.y = start.y - height + 1;
                    if (CheckLeft(wGrid, start) + 1 < height - 2)
                    {
                        shift = rand.Next(1, CheckLeft(wGrid, start) + 1);
                    }
                    else 
                    {
                        shift = rand.Next(1, height - 2);
                    }
                    start.x = start.x - shift;
                    return start;
                case Direction.east:
                    if (CheckAbove(wGrid, start) + 1 < width - 2)
                    {
                        shift = rand.Next(1, CheckAbove(wGrid, start) + 1);
                    }
                    else 
                    {
                        shift = rand.Next(1, width - 2);
                    }
                    start.y = start.y - shift;
                    return start;
                case Direction.south:
                    // width - 2 + 1
                    // - 2 damit Breite - 1 gilt (0 index),
                    // + 1 damit rand auch Breite -1 zählen kann
                    if (CheckLeft(wGrid, start) + 1 < height - 2)
                    {
                        shift = rand.Next(1, CheckLeft(wGrid, start) + 1);
                    }
                    else 
                    {
                        shift = rand.Next(1, height - 2);
                    }
                    start.x = start.x - shift;
                    return start;
                case Direction.west:
                    if (CheckAbove(wGrid, start) + 1 < width - 2)
                    {
                        shift = rand.Next(1, CheckAbove(wGrid, start) + 1);
                    }
                    else 
                    {
                        shift = rand.Next(1, width - 2);
                    }
                    start.y = start.y - shift;
                    start.x = start.x - width + 1;
                    return start;
            }
            return start;
        }

        // Draws a wall around the room. (Edge aligned INSIDE
        // the room). If there is a door, no wall is drawn in 
        // that space. (Doors stay doors).
        static Tile[,] GenerateWalls(int height, int width, 
        Tile[,] room, Tile[,] wGrid, Point start)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (OutOfBound(start.y + i, start.x + j))
                    {
                        break;
                    }
        // Draw northern and southern wall, do not draw over 
        // existing doors.
                    else if ((i == 0 || i == height - 1) && 
                    wGrid[start.y + i, start.x + j] == Tile.blank)
                    {
                        room[i,j] = Tile.wall;
                    }
                    else if ((i == 0 || i == height - 1) && 
                    wGrid[start.y + i, start.x + j] == Tile.blank)
                    {
                        room[i,j] = Tile.wall;
                    }
        // Draw eastern and western wall, do not draw over 
        // existing doors.
                    else if ((j == 0 || j == width - 1) && 
                    wGrid[start.y + i, start.x + j] == Tile.blank)
                    {
                        room[i,j] = Tile.wall;
                    }
                    else if ((j == 0 || j == width - 1) && 
                    wGrid[start.y + i, start.x + j] == Tile.door)
                    {
                        room[i,j] = Tile.door;
                    }
        // Draw inside of room, unless Tile is door
                    else
                    {
                        room[i,j] = Tile.floor;
                    }
                }
            }
            return room;
        }

        // Draws a door on a randomly picked wall, for a random 
        // number of walls. (Only one door per wall).
        // If there is already a door on that wall 
        // (CheckDoor[] == false), no new door is drawn on 
        // that wall.
        static Tile[,] GenerateDoors(int height, int width, 
        Tile[,]room, int numberOfDoors, Point start, List<Door> allDoors)
        {
            do
            {
                // Go to random wall.
                Array pickedWall = Enum.GetValues(typeof(Direction));
                Random rand = new Random();
                Direction randomWall = (Direction)pickedWall.GetValue
                (rand.Next(1, pickedWall.Length))!;

                // DEBUG to set only one direction  
                // randomWall = Direction.west;

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
                            thisDoor.y = start.y;
                            thisDoor.status = DoorState.unopened;

                            allDoors.Add(thisDoor);
                            break;

                        case Direction.east:
                            pickedSpot = rand.Next(1, height - 2);
                            room[pickedSpot, width - 1] = Tile.door;

                            thisDoor.direction = Direction.east;
                            thisDoor.x = width - 1 + start.x;
                            thisDoor.y = pickedSpot + start.y;
                            thisDoor.status = DoorState.unopened;

                            allDoors.Add(thisDoor);
                            break;
                            
                        case Direction.south:
                            pickedSpot = rand.Next(1, width - 2);
                            room[height - 1, pickedSpot] = Tile.door;
                            thisDoor.direction = Direction.south;
                            thisDoor.x = pickedSpot + start.x;
                            thisDoor.y = height - 1 + start.y;
                            thisDoor.status = DoorState.unopened;

                            allDoors.Add(thisDoor);
                            break;                         
                            
                        case Direction.west:
                            pickedSpot = rand.Next(1, height - 2);
                            room[pickedSpot, 0] = Tile.door;

                            thisDoor.direction = Direction.west; 
                            thisDoor.x = start.x;
                            thisDoor.y = pickedSpot + start.y;
                            thisDoor.status = DoorState.unopened;

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
        static Tile[,] GenerateCorridors(Tile[,] wGrid, 
        List<Door> allDoors, Point start)
        {
            // Get number of doors
            int count = 0;
            foreach (Door thisDoor in allDoors)
            {
                if (thisDoor.status == DoorState.unopened)
                {
                    count++;
                }
            }

            for (int i = allDoors.Count - 1; i >= 0; i--)
            {                
                if (allDoors[allDoors.Count - 1].status == DoorState.unopened)
                {
                    start.x = allDoors[allDoors.Count - 1].x;
                    start.y = allDoors[allDoors.Count - 1].y;
                    start.direction = allDoors[allDoors.Count - 1].direction;
                    int[] space = CheckSpaceInDirection(wGrid, start);
                    var rand = new Random();
                    int length = rand.Next(Size.minCorridorLength, Size.maxCorridorLength + 1);
                    switch (allDoors[allDoors.Count - 1].direction)
                    {
                        case Direction.north:
                            if (space[0] == 0) 
                            {
                                break;
                            }
                            else if (space[0] < Size.maxCorridorLength) 
                            {
                                length = rand.Next(Size.minCorridorLength, space[0] + 1);
                            }
                            for (int j = 0; j < length; j++)
                            {
                                allDoors[allDoors.Count - 1].status = DoorState.opened;
                                wGrid[allDoors[allDoors.Count - 1].y - 1 - j, allDoors[allDoors.Count - 1].x] = Tile.corridor;
                                if (j == length - 1 && wGrid[allDoors[allDoors.Count - 1].y - 1 - j, allDoors[allDoors.Count - 1].x] != Tile.wall)
                                {
                                    wGrid[allDoors[allDoors.Count - 1].y - 1 - j, allDoors[allDoors.Count - 1].x] = Tile.door;
                                    Door thisDoor = new Door
                                    {
                                        direction = Direction.north,
                                        x = allDoors[allDoors.Count - 1].x,
                                        y = allDoors[allDoors.Count - 1].y - 1 - j,
                                        status = DoorState.corridorEnd
                                    };
                                    allDoors.Add(thisDoor);
                                }
                            }
                            break;       
                        case Direction.east:
                            if (space[1] == 0) 
                            {
                                break;
                            }
                            else if (space[1] < Size.maxCorridorLength) 
                            {
                                length = rand.Next(Size.minCorridorLength, space[1] + 1);
                            }
                            for (int j = 0; j < length; j++)
                            {
                                allDoors[allDoors.Count - 1].status = DoorState.opened;
                                wGrid[allDoors[allDoors.Count - 1].y, allDoors[allDoors.Count - 1].x + 1 + j] = Tile.corridor;
                                if (j == length - 1 &&  wGrid[allDoors[allDoors.Count - 1].y, allDoors[allDoors.Count - 1].x + 1 + j] != Tile.wall)
                                {
                                    wGrid[allDoors[allDoors.Count - 1].y, allDoors[allDoors.Count - 1].x + 1 + j] = Tile.door;
                                    Door thisDoor = new Door
                                    {
                                        direction = Direction.east,
                                        x = allDoors[allDoors.Count - 1].x + 1 + j,
                                        y = allDoors[allDoors.Count - 1].y,
                                        status = DoorState.corridorEnd
                                    };
                                    allDoors.Add(thisDoor);
                                }
                            }
                            break;
                        case Direction.south:
                            if (space[0] == 0) 
                            {
                                break;
                            }
                            else if (space[0] < Size.maxCorridorLength) 
                            {
                                length = rand.Next(Size.minCorridorLength, space[0] + 1);
                            }
                            for (int j = 0; j < length; j++)
                            {
                                allDoors[allDoors.Count - 1].status = DoorState.opened;
                                wGrid[allDoors[allDoors.Count - 1].y + 1 + j, allDoors[allDoors.Count - 1].x] = Tile.corridor;
                                if (j == length - 1 && wGrid[allDoors[allDoors.Count - 1].y + 1 + j, allDoors[allDoors.Count - 1].x] != Tile.wall)
                                {
                                    wGrid[allDoors[allDoors.Count - 1].y + 1 + j, allDoors[allDoors.Count - 1].x] = Tile.door;
                                    Door thisDoor = new Door
                                    {
                                        direction = Direction.south,
                                        x = allDoors[allDoors.Count - 1].x,
                                        y = allDoors[allDoors.Count - 1].y + 1 + j,
                                        status = DoorState.corridorEnd
                                    };
                                    allDoors.Add(thisDoor);
                                }
                            }
                            break;
                        case Direction.west:
                            if (space[1] == 0) 
                            {
                                break;
                            }
                            else if (space[1] < Size.maxCorridorLength) 
                            {
                                length = rand.Next(Size.minCorridorLength, space[1] + 1);
                            }
                            for (int j = 0; j < length; j++)
                            {
                                allDoors[allDoors.Count - 1].status = DoorState.opened;
                                wGrid[allDoors[allDoors.Count - 1].y, allDoors[allDoors.Count - 1].x - 1 - j] = Tile.corridor;
                                if (j == length - 1 && wGrid[allDoors[allDoors.Count - 1].y, allDoors[allDoors.Count - 1].x - 1 - j] != Tile.wall)
                                {
                                    wGrid[allDoors[allDoors.Count - 1].y, allDoors[allDoors.Count - 1].x - 1 - j] = Tile.door;
                                    Door thisDoor = new Door
                                    {
                                        direction = Direction.west,
                                        x = allDoors[allDoors.Count - 1].x - 1 - j,
                                        y = allDoors[allDoors.Count - 1].y,
                                        status = DoorState.corridorEnd
                                    };
                                    allDoors.Add(thisDoor);
                                }
                            }
                            break;
                    }
                }
            }
            return wGrid;
        }

        // Checks if a wall in a given direction has a door or not.
        static bool CheckDoor(Direction wall, int height, 
        int width, Tile[,] room)
       {
            switch (wall)
            {
                case Direction.north:
                    for (int i = 0; i < width - 1; i++)
                    {
                        if (room[0, i] == Tile.door)
                        {
                            return true;
                        }
                    }
                    return false;
                case Direction.east:
                    for (int i = 0; i < height - 1; i++)
                    {
                        if (room[i, width - 1] == Tile.door)
                        {
                            return true;
                        }
                    }
                    return false;
                case Direction.south:
                    for (int i = 0; i < width - 1; i++)
                    {
                        if (room[height - 1,i] == Tile.door)
                        {
                            return true;
                        }
                    }
                    return false;
                case Direction.west:
                    for (int i = 0; i < height - 1; i++)
                    {
                        if (room[i, 0] == Tile.door)
                        {
                            return true;
                        }
                    }
                    return false;
                default:
                    return true;
            }
        } 

        // Pastes the room into the working Grid.
        static Tile[,] PasteToWorkGrid(int height, int width, 
        Tile[,] room, Tile[,] wGrid, Point start)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if(OutOfBound(start.y + i, start.x + j))
                    {
                        return wGrid;
                    }
                    else if (wGrid[start.y + i, start.x + j] == Tile.door)
                    {
                        wGrid[start.y + i, start.x + j] = wGrid[start.y + i, start.x + j];
                    }
                    else 
                    {
                        wGrid[start.y + i, start.x + j] = room[i,j];
                    }
                }
            }
            return wGrid;
        }

        static bool OutOfBound(int y, int x)
        {
            if (x < 0 || x > Size.canvasW - 1 || y < 0 || y > Size.canvasH - 1)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        // Prints the Dungeon. (Currently using the working grid)
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
                            Console.Write("_");
                            break;  
                    }
                }
                Console.Write("\n");
            }
        }
    }
}
