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
    protected override void generateDungeon()
    {
        tileMapVisualizer.Clear();

        RoomGenerator roomGenerator = new RoomGenerator(minRoomWidth, minRoomHeight, buffer, roomChance, dimensionRatio);
        List<BoundsInt> rooms = roomGenerator.createRooms(dungeonWidth, dungeonHeight);

        PathGenerator pathGenerator = new PathGenerator(buffer);
        HashSet<Vector2Int> paths = pathGenerator.createPaths(rooms);

        FloorGenerator floorGenerator = new FloorGenerator(tileMapVisualizer, paths, rooms, buffer);
        floorGenerator.generateFloors();
    }
}