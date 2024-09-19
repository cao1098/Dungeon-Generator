using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathGenerator
{
    private int buffer;

    public PathGenerator(int buffer) { 
        this.buffer = buffer;
    }

    public HashSet<Vector2Int> createPaths(List<BoundsInt> roomList) {
        
        HashSet<Vector2Int> paths = new HashSet<Vector2Int>();

        List<BoundsInt> connectedRooms = new List<BoundsInt>();

        BoundsInt currRoom = roomList.First();
        connectedRooms.Add(currRoom);

        while (connectedRooms.Count <= roomList.Count)
        {
            BoundsInt closestRoom = findClosestRoom(currRoom, roomList, connectedRooms);

            //Vector2Int startPoint = getRandomPerimeterPoint(currRoom);
            //Vector2Int endPoint = getRandomPerimeterPoint(closestRoom);

            //KEEP FOR DEBUGGING PURPOSES
            Vector2Int startPoint = new Vector2Int((int)currRoom.center.x, (int)currRoom.center.y);
            Vector2Int endPoint = new Vector2Int((int)closestRoom.center.x, (int)closestRoom.center.y);

            paths.UnionWith(drawPath(startPoint, endPoint, paths));

            connectedRooms.Add(closestRoom);

            currRoom = closestRoom;
        }
        return paths;
    }

    private HashSet<Vector2Int> drawPath(Vector2Int startPos, Vector2Int endPos, HashSet<Vector2Int> paths)
    {
        int step = startPos.x <= endPos.x ? 1 : -1;

        for (int x = startPos.x; x != endPos.x + step; x += step)
        {
            Vector2Int position = new Vector2Int(x, startPos.y);
            paths.Add(position);
        }

        step = startPos.y <= endPos.y ? 1 : -1;

        for (int y = startPos.y; y != endPos.y + step; y += step)
        {
            Vector2Int position = new Vector2Int(endPos.x, y);
            paths.Add(position);
        }

        return paths;
    }

    private Vector2Int getRandomPerimeterPoint(BoundsInt currRoom)
    {
        throw new NotImplementedException();
    }

    private BoundsInt findClosestRoom(BoundsInt room, List<BoundsInt> roomList, List<BoundsInt> connectedRooms)
    {
        BoundsInt neighbor = roomList.First();

        double currDistance = getDistance(room.center, neighbor.center);

        if (currDistance == 0 || connectedRooms.Contains(neighbor)) { 
            currDistance = double.MaxValue;
        }

        foreach (var possibleNeighbor in roomList) {
            if (!connectedRooms.Contains(possibleNeighbor)) {
                double possibleDistance = getDistance(room.center, possibleNeighbor.center);
                if (possibleDistance < currDistance && possibleDistance != 0) {
                    neighbor = possibleNeighbor;
                    currDistance = possibleDistance;
                }
            }
        }

        return neighbor;
    }

    private double getDistance(Vector3 center1, Vector3 center2)
    {
        double X = center1.x - center2.x;
        double Y = center1.y - center2.y;
        return System.Math.Abs(System.Math.Sqrt(X * X + Y * Y));
    }
}
