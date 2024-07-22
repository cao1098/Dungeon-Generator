using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonGeneratorAlgorithm : AbstractDungeonGenerator
{

    [SerializeField] private TileMapVisualizer tileMapVisualizer;
    [SerializeField] private int dungeonHeight;
    [SerializeField] private int dungeonWidth;

    [SerializeField] private int minRoomWidth;
    [SerializeField] private int minRoomHeight;

    [SerializeField] private int buffer;
    [SerializeField] private int roomChance;
    [SerializeField] private int dimensionRatio;

    protected override void generateDungeon() {
        //Clear the tiles
        tileMapVisualizer.Clear();

        //Queue holds potential rooms, roomList holds rooms
        Queue roomQueue = new Queue();
        List<BoundsInt> roomList = new List<BoundsInt>();

        //Create and enqueue the first room
        var dungeon = new BoundsInt(Vector3Int.zero, new Vector3Int(dungeonWidth, dungeonHeight, 0));
        roomQueue.Enqueue(dungeon);

        //Loop through the queue until there are no more potential rooms
        while (roomQueue.Count > 0) {
            //Take out room to be split
            BoundsInt room = (BoundsInt)roomQueue.Dequeue();
           // Debug.Log("Room Min: " + room.min.ToString() + " Max: " + room.max.ToString());

            //Check to make sure room is big enough to be split
            if (room.size.y >= minRoomHeight && room.size.x >= minRoomWidth)
            {
                //Select vertical or horizontal split
                int split = Random.Range(0, 2);
                
                if (split == 0)
                {
                    //Vertical
                    
                    if (room.size.y >= minRoomHeight * 2)
                    {
                        splitHorizontal(room, roomQueue, buffer);
                    }
                    else if (room.size.x >= minRoomWidth * 2)
                    {
                        splitVertical(room, roomQueue, buffer);
                    }
                    else
                    {
                        var roomOdds = Random.Range(0, 10);
                        if (roomOdds < roomChance)
                            roomList.Add(room);
                    }
                }
                else
                {
                    if (room.size.x >= minRoomWidth * 2)
                    {
                        splitVertical(room, roomQueue, buffer);
                    }
                    else if (room.size.y >= minRoomHeight * 2)
                    {
                        splitHorizontal(room, roomQueue, buffer);
                    }
                    else
                    {
                        var roomOdds = Random.Range(0, 10);
                        if (roomOdds < roomChance)
                            roomList.Add(room);
                    }
                }
            }
            else {
               // Debug.Log("Adding to room list");
                var roomOdds = Random.Range(0, 10);
                if (roomOdds < roomChance)
                    roomList.Add(room);
            }
        }
        //Check to make sure no rooms overlap with each other
        roomList.RemoveAll(room => room.size.y + buffer > room.size.x * dimensionRatio || room.size.x + buffer > room.size.y * dimensionRatio);

        //Send roomList to be tiled.
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        floor = createRoom(roomList);
        HashSet<Vector2Int> paths = new HashSet<Vector2Int>();


        paths = createPath(roomList);
        tileMapVisualizer.PaintFloorTiles(floor);
        tileMapVisualizer.PaintFloorTiles(paths);
    }


    //Splits a room vertically
    private static void splitVertical(BoundsInt room, Queue roomQueue, int buffer) {
       // Debug.Log("Vertical split");
        var splitX = Random.Range(room.xMin + buffer, room.xMax - buffer);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(splitX - room.xMin - buffer, room.size.y, 0));
        BoundsInt room2 = new BoundsInt(new Vector3Int(splitX, room.yMin, 0), new Vector3Int(room.xMax - splitX, room.size.y, 0));

        roomQueue.Enqueue(room1);
        roomQueue.Enqueue(room2);
    }

    //Splits a room horizontally
    private static void splitHorizontal(BoundsInt room, Queue roomQueue, int buffer) {
        //Debug.Log("Horizontal split");
        var splitY = Random.Range(room.yMin + buffer, room.yMax - buffer);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, splitY - room.yMin - buffer));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.xMin, splitY), new Vector3Int(room.size.x, room.yMax - splitY));


        roomQueue.Enqueue(room1);
        roomQueue.Enqueue(room2);
    }

    private HashSet<Vector2Int> createPath(List<BoundsInt> roomList) {
        Debug.Log("hi");

        HashSet<Vector2Int> paths = new HashSet<Vector2Int>(); //to hold all the paths

        List<BoundsInt> connectedRooms = new List<BoundsInt>(); //to hold the rooms that have been connected

        BoundsInt currRoom = roomList.First(); //the first room
        connectedRooms.Add(currRoom);

        while (connectedRooms.Count <= roomList.Count) {
            Debug.Log("Connected: " + connectedRooms.Count.ToString() + " List: " +  roomList.Count.ToString());
            BoundsInt closestRoom = findClosestRoom(currRoom, roomList, connectedRooms);

            //CREATE THE PATH
            paths.UnionWith(drawPath(currRoom, closestRoom, paths));

            connectedRooms.Add(closestRoom);

            currRoom = closestRoom;
        }
        return paths;
    }

    private HashSet<Vector2Int> drawPath(BoundsInt currRoom, BoundsInt closestRoom, HashSet<Vector2Int> paths)
    {
        Debug.Log("Drawing the path");

        //Select a random startposition in current room
        //Select a random end position in closest room
        //Rightmost point should be endPos, leftmost point should be startPos

        Vector2Int startingPosition = getRandomPerimeterPoint(currRoom);
        Vector2Int endingPosition = getRandomPerimeterPoint(closestRoom);

        startingPosition = new Vector2Int((int)currRoom.center.x, (int)currRoom.center.y);
        endingPosition = new Vector2Int((int)closestRoom.center.x, (int)closestRoom.center.y);
        /*
        if (startingPosition.x == currRoom.xMax - buffer)
        {
            Debug.Log("Right");
           // 
            for (int i = 0; i < 3; i++) {
                paths.Add(new Vector2Int(startingPosition.x + i, (int)startingPosition.y));
            }
            startingPosition.x += 3;
        }
        else if (startingPosition.x == currRoom.xMin + buffer)
        {
            Debug.Log("Left");
            //
            for (int i = 0; i < 3; i++)
            {
                paths.Add(new Vector2Int(startingPosition.x - i, (int)startingPosition.y));
            }
            startingPosition.x -= 3;
        }
        else if (startingPosition.y == currRoom.yMax - buffer)
        {
            Debug.Log("Top");
            //  
            for (int i = 0; i < 3; i++)
            {
                paths.Add(new Vector2Int(endingPosition.x, (int)startingPosition.y + i));
            }
            startingPosition.y += 3;

        }
        else if (startingPosition.y == currRoom.yMin + buffer)
        {
            Debug.Log("Bottom");
            
            for (int i = 0; i < 3; i++)
            {
                paths.Add(new Vector2Int(endingPosition.x, (int)startingPosition.y - i));
            }
            startingPosition.y -= 3;
        }*/

        if (startingPosition.x > endingPosition.x) {
            (startingPosition, endingPosition) = (endingPosition, startingPosition); //swap to make
        }

        

        Debug.Log("Starting X at: " + startingPosition.ToString() + " Ending X at: " + endingPosition.ToString());
        for (int x = (int)startingPosition.x; x <= endingPosition.x; x++)
        {
            Vector2Int position = new Vector2Int(x, (int)startingPosition.y);
            paths.Add(position);

        }
        var currX = endingPosition.x;

        if (startingPosition.y > endingPosition.y) {
            (startingPosition, endingPosition) = (endingPosition, startingPosition);
        }

        Debug.Log("Starting Y at: " + startingPosition.ToString() + " Ending Y at: (" + currX.ToString() + ", " + endingPosition.y.ToString());
        for (int y = (int)startingPosition.y; y <= endingPosition.y; y++)
        {
            Vector2Int position = new Vector2Int((int)currX, y);
            paths.Add(position);
        }

        return paths;
    }

    private Vector2Int getRandomPerimeterPoint(BoundsInt currRoom)
    {
        List<Vector2Int> perimeter = new List<Vector2Int>();
        for (int x = currRoom.xMin + buffer; x < currRoom.xMax - buffer; x++) {
            perimeter.Add(new Vector2Int(x, currRoom.yMin + buffer));
            perimeter.Add(new Vector2Int(x, currRoom.yMax - buffer));
        }

        for (int y = currRoom.yMin + buffer; y < currRoom.yMax - buffer; y++) {
            perimeter.Add(new Vector2Int(currRoom.xMin + buffer, y));
            perimeter.Add(new Vector2Int(currRoom.xMax - buffer, y ));
        }

        //perimeter.ToArray();
        int pos = Random.Range(0, perimeter.Count);

        Debug.Log("Point is: " + perimeter[pos].ToString());
        return perimeter[pos];
    }

    private BoundsInt findClosestRoom(BoundsInt room, List<BoundsInt> roomList, List<BoundsInt> connectedRooms)
    {
        //Needs to be reworked for connectedRooms.
        BoundsInt neighbor = roomList.First();
        double currDistance = getDistance(room.center, neighbor.center);
        if (currDistance == 0 || connectedRooms.Contains(neighbor))  
            currDistance = double.MaxValue;
        
        foreach (var possibleNeighbor in roomList) {
            if (getDistance(room.center, possibleNeighbor.center) < currDistance && getDistance(room.center, possibleNeighbor.center) != 0 && !connectedRooms.Contains(possibleNeighbor)) { 
                neighbor = possibleNeighbor;
                currDistance = getDistance(room.center, possibleNeighbor.center);
            }
        }
        Debug.Log("Closest point to: " + room.ToString() + " is: " + neighbor.ToString());
        return neighbor;
    }

    private double getDistance(Vector3 center1, Vector3 center2)
    {
        double X = center1.x - center2.x;
        double Y = center1.y - center2.y;
        return System.Math.Abs(System.Math.Sqrt(X * X + Y * Y));
    }

    //Creates the floor positions for the tiles of the rooms.
    //BoundsInt roomList - list of rooms in the dungeon
    //@return floor, list of positions that need to be tiled.
    private HashSet<Vector2Int> createRoom(List<BoundsInt> roomList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        foreach (var room in roomList) {
                for (int i = buffer; i < room.size.x - buffer; i++) {
                    for (int j = buffer; j < room.size.y - buffer; j++) {
                        Vector2Int position = (Vector2Int)room.min + new Vector2Int(i, j);
                        floor.Add(position);
                    }
                }
        }
        return floor;
    }
}
