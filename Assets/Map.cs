using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

enum Tile : int
{
    Floor = 0,
    Water = 1,
    BridgeN = 2,
    BridgeE = 3
}

struct TileInfo
{
    public int Height;
    public Tile Type;

    public static bool operator >=(TileInfo a, TileInfo b)
    {
        return a.Height >= b.Height;
    }

    public static bool operator <=(TileInfo a, TileInfo b)
    {
        return false;
    }
}

[System.Serializable]
public struct PerlinNoise
{
    public Vector3 Scale;
    public AnimationCurve Distribution;

    public float Evaluate(int x, int y)
    {
        return Distribution.Evaluate(Mathf.PerlinNoise(x*Scale.x,y*Scale.y)) * Scale.z;
    }
}

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Map : MonoBehaviour 
{
    public Sprite[] Tiles;

	void Update() 
	{
        if (Tiles == null || Tiles.Length == 0)
            return;

        TileInfo[,] level = MakeLevel();//ParseLevel(Level.text);

        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = Tiles[0].texture;

        if (GetComponent<MeshFilter>().sharedMesh == null)
            GetComponent<MeshFilter>().sharedMesh = new Mesh();
        GenerateMesh(GetComponent<MeshFilter>().sharedMesh, level, Tiles);
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
        int idx = (Mathf.Abs(maxx - minx) > 0 ? 110 : 100) + roomIndex;
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

    TileInfo[,] MakeLevel()
    {
        random = new System.Random(0);

        const int roomWidth = 6, roomSize = 10, roomsToMake = 10;
        int width = roomWidth * roomSize;
        int[,] rooms = new int[roomWidth, roomWidth];
        int[,] level = new int[width, width];

        RoomEdge currentRoom = new RoomEdge(roomWidth / 2, roomWidth-1, 0, 0, 0);
        List<RoomEdge> availableEdges = new List<RoomEdge>();
        for (int ind = 0; ind<roomsToMake+1; ind++)
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
        return finalLevel;
    }

    static int GetEdgeIndex(int x, int y, TileInfo[,] level, TileInfo current)
    {
        int mx = level.GetLength(0) - 1, my = level.GetLength(1) - 1;

        bool l = x > 0 && level[x-1,y] >= current;
        bool t = y > 0 && level[x,y-1] >= current;

        bool r = x < mx && level[x+1,y] >= current;
        bool b = y < my && level[x,y+1] >= current;

        bool tl = x > 0  && y > 0  && level[x-1,y-1] >= current;
        bool tr = x < mx && y > 0  && level[x+1,y-1] >= current;
        bool bl = x > 0  && y < my && level[x-1,y+1] >= current;
        bool br = x < mx && y < my && level[x+1,y+1] >= current;

        int idx = 0;
        if (l && tl && t) idx += 1;
        if (t && tr && r) idx += 2;
        if (r && br && b) idx += 8;
        if (b && bl && l) idx += 4;

        return idx;
    }

    static int GetHeight(TileInfo tile)
    {
        if (tile.Type == Tile.BridgeE || tile.Type == Tile.BridgeN)
            return 0;
        else
            return tile.Height;
    }

    static void GenerateMesh(Mesh mesh, TileInfo[,] level, Sprite[] tiles)
    {
		MeshProxy meshProxy = new MeshProxy();

        for (int x = 0; x<level.GetLength(0); x++)
        {
            for (int y = 0; y<level.GetLength(1); y++)
            {
                TileInfo current = level[x,y];
                if (current.Height == 0)
                    continue;

                if (current.Type == Tile.BridgeN || current.Type == Tile.BridgeE)
                {
                    meshProxy.AddQuad(tiles, current.Type == Tile.BridgeN ? 34 : 35,
                                      new Vector3(x-0.1f,current.Height+((x+y)*0.0001f),-y+0.1f),
                                      -Vector3.forward*1.2f,
                                      Vector3.right*1.2f,
                                      Vector3.up);
                    continue;
                }

                //Create top tile
                int ind = GetEdgeIndex(x,y,level,level[x,y]);
                if (ind == 0)
                    ind = 15;
                if (current.Type == Tile.Water) 
                    ind += 15;

                meshProxy.AddQuad(tiles, ind-1,
                                  new Vector3(x,current.Height,-y),
                                  -Vector3.forward,
                                  Vector3.right,
                                  Vector3.up);

                //Create tile sides
                int mx = level.GetLength(0) - 1, my = level.GetLength(1) - 1;
                for (int c = current.Height; c>=0; c--)
                {
                    int sideInd = 32;
                    if (c > 1) sideInd = 31;
                    if (c > 2) sideInd = 30;

                    if (c > (x == 0 ? 0 : GetHeight(level[x-1,y])))
                    {
                        meshProxy.AddQuad(tiles, sideInd,
                                      new Vector3(x,c-1,-y-1),
                                      Vector3.up,
                                      Vector3.forward,
                                      -Vector3.right);
                    }
                    if (c > (x == mx ? 0 : GetHeight(level[x+1,y])))
                    {
                        meshProxy.AddQuad(tiles, sideInd,
                                          new Vector3(x+1,c-1,-y),
                                          Vector3.up,
                                          -Vector3.forward,
                                          Vector3.right);
                    }
                    if (c > (y == 0 ? 0 : GetHeight(level[x,y-1])))
                    {
                        meshProxy.AddQuad(tiles, sideInd,
                                          new Vector3(x,c-1,-y),
                                          Vector3.up,
                                          Vector3.right,
                                          Vector3.forward);
                    }
                    if (c > (y == my ? 0 : GetHeight(level[x,y+1])))
                    {
                        meshProxy.AddQuad(tiles, sideInd,
                                          new Vector3(x+1,c-1,-y-1),
                                          Vector3.up,
                                          -Vector3.right,
                                          -Vector3.forward);
                    }
                }
            }
        }

        meshProxy.ToMesh(mesh);
    }
}
