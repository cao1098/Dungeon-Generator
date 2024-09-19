using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator
{
    private int minRoomWidth;
    private int minRoomHeight;

    private int buffer;
    private int roomChance;
    private int dimensionRatio;

    public RoomGenerator(int minRoomWidth, int minRoomHeight, int buffer, int roomChance, int dimensionRatio) { 
        this.minRoomWidth = minRoomWidth;
        this.minRoomHeight = minRoomHeight;
        this.buffer = buffer;
        this.roomChance = roomChance;
        this.dimensionRatio = dimensionRatio;
    }

    public List<BoundsInt> createRooms(int dungeonWidth, int dungeonHeight)
    {
        Queue roomQueue = new Queue(); //Potential Rooms
        List<BoundsInt> roomList = new List<BoundsInt>(); //Created Rooms

        var dungeon = new BoundsInt(Vector3Int.zero, new Vector3Int(dungeonWidth, dungeonHeight, 0));
        roomQueue.Enqueue(dungeon);

        while (roomQueue.Count > 0)
        {
            BoundsInt room = (BoundsInt)roomQueue.Dequeue();

            if (room.size.y >= minRoomHeight && room.size.x >= minRoomWidth)
            {
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
                        chanceToGenerate(roomChance, roomList, room);
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
                        chanceToGenerate(roomChance, roomList, room);

                    }
                }
            }
            else {
                chanceToGenerate(roomChance, roomList, room);
            }
        }
        roomList.RemoveAll(room => room.size.y + buffer > room.size.x * dimensionRatio 
        || room.size.x + buffer > room.size.y * dimensionRatio);

        return roomList;
    }

    private void chanceToGenerate(int roomChance, List<BoundsInt> roomList, BoundsInt room)
    {
        var roomOdds = Random.Range(0, 10);
        if (roomOdds < roomChance)
        {
            roomList.Add(room);
        }
    }

    private void splitVertical(BoundsInt room, Queue roomQueue, int buffer) {
        var splitX = Random.Range(room.xMin + buffer, room.xMax - buffer);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(splitX - room.xMin - buffer, room.size.y, 0));
        BoundsInt room2 = new BoundsInt(new Vector3Int(splitX, room.yMin, 0), new Vector3Int(room.xMax - splitX, room.size.y, 0));

        roomQueue.Enqueue(room1);
        roomQueue.Enqueue(room2);
    }

    private void splitHorizontal(BoundsInt room, Queue roomQueue, int buffer) {
        var splitY = Random.Range(room.yMin + buffer, room.yMax - buffer);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, splitY - room.yMin - buffer));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.xMin, splitY), new Vector3Int(room.size.x, room.yMax - splitY));

        roomQueue.Enqueue(room1);
        roomQueue.Enqueue(room2);
    }


}
