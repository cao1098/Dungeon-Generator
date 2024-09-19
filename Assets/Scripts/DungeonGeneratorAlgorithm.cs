using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
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

    //Generates the dungeon
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

            //Check to make sure room is big enough to be split
            if (room.size.y >= minRoomHeight && room.size.x >= minRoomWidth)
            {
                //Select vertical or horizontal split
                int split = Random.Range(0, 2);
                
                if (split == 0)
                {
                    
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

    //Creates the path between two rooms
    private HashSet<Vector2Int> createPath(List<BoundsInt> roomList) {
        Debug.Log("hi");

        HashSet<Vector2Int> paths = new HashSet<Vector2Int>(); //to hold all the paths

        List<BoundsInt> connectedRooms = new List<BoundsInt>(); //to hold the rooms that have been connected

        BoundsInt currRoom = roomList.First(); //the first room
        connectedRooms.Add(currRoom);

        while (connectedRooms.Count <= roomList.Count) {
            Debug.Log("Connected: " + connectedRooms.Count.ToString() + " List: " +  roomList.Count.ToString());
            BoundsInt closestRoom = findClosestRoom(currRoom, roomList, connectedRooms);

            
            Vector2Int startPoint = getRandomPerimeterPoint(currRoom);
            Vector2Int endPoint = getRandomPerimeterPoint(closestRoom);


            startPoint = new Vector2Int((int)currRoom.center.x, (int)currRoom.center.y);
            endPoint = new Vector2Int((int)closestRoom.center.x, (int)closestRoom.center.y);

            //CREATE THE PATH
            paths.UnionWith(drawPath(startPoint, endPoint, paths));

            connectedRooms.Add(closestRoom);

            currRoom = closestRoom;
        }
        return paths;
    }

    //Draws the coordinate path between two rooms
    private HashSet<Vector2Int> drawPath(Vector2Int startPos, Vector2Int endPos, HashSet<Vector2Int> paths)
    {
        Debug.Log("Drawing the path");

        //Align X-axis
        int step = startPos.x <= endPos.x ? 1 : -1;

        // Loop from startPos.x to endPos.x, adjusting for direction
        for (int x = startPos.x; x != endPos.x + step; x += step)
        {
            Vector2Int position = new Vector2Int(x, startPos.y);
            paths.Add(position);
        }

        //Align Y-axis
        step = startPos.y <= endPos.y ? 1 : -1;

        //Loop
        for (int y = startPos.y; y != endPos.y + step; y += step) {
            Vector2Int position = new Vector2Int(endPos.x, y);
            paths.Add(position);
        }

        return paths;
    }

    //Gets a random point on the perimeter of a room
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

    //Finds the room closest to the current room provided it has not been visited before
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

    //Gets the distance between two points
    private double getDistance(Vector3 center1, Vector3 center2)
    {
        double X = center1.x - center2.x;
        double Y = center1.y - center2.y;
        return System.Math.Abs(System.Math.Sqrt(X * X + Y * Y));
    }

    
    //Creates the floor positions for the tiles of the rooms.
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
