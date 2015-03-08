using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public enum Tile : int
{
    Floor,
    Water,
    BridgeN,
    BridgeE,
    RampN,
    RampE,
    RampS,
    RampW
}

public struct TileInfo
{
    public int Height;
    public Tile Type;

    public static bool operator >=(TileInfo a, TileInfo b)
    {
        return a.Height >= b.Height;
    }

    public static bool operator <=(TileInfo a, TileInfo b)
    {
        return a.Height <= b.Height;
    }
}

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Map : MonoBehaviour 
{
    public Sprite[] Tiles;
    public TileInfo[,] Level { get; private set; }

    public TileInfo Get(int x, int y)
    {
        if (Level != null && x >= 0 && y >= 0 && x < Level.GetLength(0) && y < Level.GetLength(1))
            return Level[x,y];
        else
            return new TileInfo();
    }

	void Update() 
	{
        if (Tiles == null || Tiles.Length == 0)
            return;

        TileInfo[,] level = MakeLevel();

        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Custom/Shader"));
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = Tiles[0].texture;

        if (GetComponent<MeshFilter>().sharedMesh == null)
            GetComponent<MeshFilter>().sharedMesh = new Mesh();
        MapMesh.GenerateMesh(GetComponent<MeshFilter>().sharedMesh, level, Tiles);

        Level = level;
	}

    struct TileEdge
    {
        public int x, y, numEdges;

        public TileEdge(int x, int y, int numEdges)
        {
            this.x = x;
            this.y = y;
            this.numEdges = numEdges;
        }
    }

    void MakeBridge(int[,] level, int ax, int ay, int bx, int by, int roomIndex)
    {
        int minx = Mathf.Min(ax, bx), miny = Mathf.Min(ay, by);
        int maxx = Mathf.Max(ax, bx), maxy = Mathf.Max(ay, by);

        bool isForward = Mathf.Abs(maxy - miny) > 0;

        int idx = (isForward ? 100 : 110) + roomIndex;
        for (int x = minx; x<=maxx; x++)
            for (int y = miny; y<=maxy; y++)
                if (level[x,y] <= 0)
                    level[x,y] = idx;
    }

    void MakeRoom(int[,] level, int roomIndex, int numTiles, int messyness, int sx, int sy, int width)
    {
        level[sx+(width/2),sy+(width/2)] = roomIndex;

        sx = Mathf.Max(1, sx - width);
        sy = Mathf.Max(1, sy - width);
        width = Mathf.Min(
            Mathf.Min(level.GetLength(0) - sx, (width * 3)),
            Mathf.Min(level.GetLength(1) - sy, (width * 3))
        );

        List<TileEdge> potentials = new List<TileEdge>();
        while (numTiles > 0)
        {
            //Find potential new positions
            potentials.Clear();
            for (int x = sx+1; x<sx+width-1; x++)
            {
                for (int y = sy+1; y<sy+width-1; y++)
                {
                    if (level[x,y] != 0)
                        continue;
                    int numEdges = 
                        (level[x-1,y] == roomIndex ? 1 : 0) +
                        (level[x+1,y] == roomIndex ? 1 : 0) +
                        (level[x,y-1] == roomIndex ? 1 : 0) +
                        (level[x,y+1] == roomIndex ? 1 : 0);
                    if (numEdges == 4)
                        level[x,y] = roomIndex;
                    else if (numEdges > 0)
                        potentials.Add(new TileEdge(x,y,numEdges));
                }
            }
            
            //Randomly pick a new potential cell
            if (potentials.Count == 0)
                break;
            
            potentials.Sort((a,b) => b.numEdges.CompareTo(a.numEdges));
            TileEdge edge = potentials[Range(0,Mathf.Min(potentials.Count,messyness))];
            level[edge.x,edge.y] = roomIndex;
            numTiles--;
        }

        //Mark all 8 edges that are not the room as unusable
        for (int x = 1; x<level.GetLength(0)-1; x++)
        {
            for (int y = 1; y<level.GetLength(0)-1; y++)
            {
                if (level[x,y] != roomIndex)
                    continue;

                if (level[x-1,y] == 0) level[x-1,y] = -roomIndex;
                if (level[x+1,y]== 0) level[x+1,y] = -roomIndex;
                if (level[x,y-1]== 0) level[x,y-1] = -roomIndex;
                if (level[x,y+1]== 0) level[x,y+1] = -roomIndex;

                if (level[x-1,y-1]== 0) level[x-1,y-1] = -roomIndex;
                if (level[x+1,y-1]== 0) level[x+1,y-1] = -roomIndex;
                if (level[x+1,y+1]== 0) level[x+1,y+1] = -roomIndex;
                if (level[x-1,y+1]== 0) level[x-1,y+1] = -roomIndex;
            }
        }
    }

    struct RoomEdge
    {
        public int tx, ty, sx, sy, roomIdx;

        public RoomEdge(int tx, int ty, int sx, int sy, int roomIdx)
        {
            this.tx = tx;
            this.ty = ty;
            this.sx = sx;
            this.sy = sy;
            this.roomIdx = roomIdx;
        }
    }

    System.Random random;
    int Range(int min, int max)
    {
        return random.Next(max - min) + min;
    }

    public int Seed = 100;

    TileInfo[,] MakeLevel()
    {
        random = new System.Random(Seed);

        const int roomWidth = 6, roomSize = 10, roomsToMake = 10;
        int width = roomWidth * roomSize;
        int[,] rooms = new int[roomWidth, roomWidth];
        int[,] level = new int[width, width];

        RoomEdge currentRoom = new RoomEdge(roomWidth / 2, 0, 0, 0, 0);
        List<RoomEdge> availableEdges = new List<RoomEdge>();
        for (int ind = 0; ind<roomsToMake; ind++)
        {
            int roomIdx = (ind/5)+1;
            rooms[currentRoom.tx,currentRoom.ty] = roomIdx;
            MakeRoom(level, roomIdx, Range(40,60), Range(8,20), 
                     currentRoom.tx * roomSize, currentRoom.ty * roomSize, roomSize);

            if (ind > 0)
            {
                MakeBridge(level, 
                           currentRoom.tx * roomSize + roomSize/2,
                           currentRoom.ty * roomSize + roomSize/2,
                           currentRoom.sx * roomSize + roomSize/2, 
                           currentRoom.sy * roomSize + roomSize/2, 
                           currentRoom.roomIdx);
            }

            availableEdges.Clear();
            for (int x = 1; x<roomWidth-1; x++)
            {
                for (int y = 1; y<roomWidth-1; y++)
                {
                    if (rooms[x,y] != 0)
                        continue;
                    if (rooms[x-1,y] != 0)
                        availableEdges.Add(new RoomEdge(x,y,x-1,y,rooms[x-1,y]));
                    if (rooms[x+1,y] != 0)
                        availableEdges.Add(new RoomEdge(x,y,x+1,y,rooms[x+1,y]));
                    if (rooms[x,y-1] != 0)
                        availableEdges.Add(new RoomEdge(x,y,x,y-1,rooms[x,y-1]));
                    if (rooms[x,y+1] != 0)
                        availableEdges.Add(new RoomEdge(x,y,x,y+1,rooms[x,y+1]));
                }
            }

            if (availableEdges.Count == 0)
                break;

            currentRoom = availableEdges[Range(0,availableEdges.Count)];
        }

        //Figure out the final level types
        TileInfo[,] finalLevel = new TileInfo[width,width];
        for (int x = 0; x<width; x++)
        {
            for (int y = 0; y<width; y++)
            {
                int height = level[x,y];
                if (height < 0)
                    continue;
                if (height > 110)
                {
                    finalLevel[x,y].Type = Tile.BridgeE;
                    height -= 110;
                }
                else if (height > 100)
                {
                    finalLevel[x,y].Type = Tile.BridgeN;
                    height -= 100;
                }

                finalLevel[x,y].Height = height;
            }
        }

        //Add ramps when needed to move up near bridges
        for (int x = 1; x<width-1; x++)
        {
            for (int y = 1; y<width-1; y++)
            {
                int height = finalLevel[x, y].Height;
                if (height <= 0)
                    continue;

                if (finalLevel[x-1, y].Height > height)
                    finalLevel[x-1, y].Type = Tile.RampE;
                if (finalLevel[x+1, y].Height > height)
                    finalLevel[x+1, y].Type = Tile.RampW;
                if (finalLevel[x, y+1].Height > height)
                    finalLevel[x, y+1].Type = Tile.RampN;
                if (finalLevel[x, y-1].Height > height)
                    finalLevel[x, y-1].Type = Tile.RampS;
            }
        }

        return finalLevel;
    }
}
