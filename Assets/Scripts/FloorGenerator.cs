using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;

public class FloorGenerator
{
    private TileMapVisualizer tileMapVisualizer;
    private HashSet<Vector2Int> pathTiles;
    private List<BoundsInt> rooms;
    private int buffer;

    public FloorGenerator(TileMapVisualizer tileMapVisualizer, HashSet<Vector2Int> pathTiles, List<BoundsInt> rooms, int buffer) { 
        this.tileMapVisualizer = tileMapVisualizer;
        this.pathTiles = pathTiles;
        this.rooms = rooms;
        this.buffer = buffer;
    }

    public void generateFloors() {
        HashSet<Vector2Int> roomTiles = createFloor(rooms);
        tileMapVisualizer.PaintFloorTiles(roomTiles);
        tileMapVisualizer.PaintFloorTiles(pathTiles);
    }

    private HashSet<Vector2Int> createFloor(List<BoundsInt> roomList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        foreach (var room in roomList)
        {
            for (int i = buffer; i < room.size.x - buffer; i++)
            {
                for (int j = buffer; j < room.size.y - buffer; j++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(i, j);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }
}
