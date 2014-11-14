using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

enum Tile : int
{
    Floor = 0,
    Water = 1
}

struct TileInfo
{
    public int Height;
    public Tile Type;
}

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Map : MonoBehaviour 
{
	public TextAsset Level;
    public Sprite[] Tiles;

	void Update() 
	{
        if (Level == null || Tiles == null || Tiles.Length == 0)
            return;

        TileInfo[,] level = ParseLevel(Level.text);

        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = Tiles[0].texture;

        if (GetComponent<MeshFilter>().sharedMesh == null)
            GetComponent<MeshFilter>().sharedMesh = new Mesh();
        GenerateMesh(GetComponent<MeshFilter>().sharedMesh, level, Tiles);
	}

    static TileInfo[,] ParseLevel(string level)
    {
        string[] lines = level.Split('\n');
        int width = lines[0].Length, height = lines.Length;

        TileInfo[,] tiles = new TileInfo[width,height];
		for (int y = 0; y<height; y++)
        {
			for (int x = 0; x<width&&x<lines[y].Length; x++)
            {
                tiles[x,y].Type = Tile.Floor;
                tiles[x,y].Height = (int)lines[y][x] - 48;
            }
        }

        return tiles;
    }

    static int GetEdgeIndex(int x, int y, TileInfo[,] level, TileInfo current)
    {
        int mx = level.GetLength(0) - 1, my = level.GetLength(1) - 1;

        bool l = x > 0 && level[x-1,y].Height >= current.Height;
        bool t = y > 0 && level[x,y-1].Height >= current.Height;

        bool r = x < mx && level[x+1,y].Height >= current.Height;
        bool b = y < my && level[x,y+1].Height >= current.Height;

        bool tl = x > 0  && y > 0  && level[x-1,y-1].Height >= current.Height;
        bool tr = x < mx && y > 0  && level[x+1,y-1].Height >= current.Height;
        bool bl = x > 0  && y < my && level[x-1,y+1].Height >= current.Height;
        bool br = x < mx && y < my && level[x+1,y+1].Height >= current.Height;

        int idx = 0;
        if (l && tl && t) idx += 1;
        if (t && tr && r) idx += 2;
        if (r && br && b) idx += 8;
        if (b && bl && l) idx += 4;

        return idx;
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
                if (current.Height > (x == 0 ? 0 : level[x-1,y].Height))
                {
                    meshProxy.AddQuad(tiles, 30,
                                  new Vector3(x,current.Height-1,-y-1),
                                  Vector3.up,
                                  Vector3.forward,
                                  -Vector3.right);
                }
                if (current.Height > (x == mx ? 0 : level[x+1,y].Height))
                {
                    meshProxy.AddQuad(tiles, 30,
                                      new Vector3(x+1,current.Height-1,-y),
                                      Vector3.up,
                                      -Vector3.forward,
                                      Vector3.right);
                }
                if (current.Height > (y == 0 ? 0 : level[x,y-1].Height))
                {
                    meshProxy.AddQuad(tiles, 30,
                                      new Vector3(x,current.Height-1,-y),
                                      Vector3.up,
                                      Vector3.right,
                                      Vector3.forward);
                }
                if (current.Height > (y == my ? 0 : level[x,y+1].Height))
                {
                    meshProxy.AddQuad(tiles, 30,
                                      new Vector3(x+1,current.Height-1,-y-1),
                                      Vector3.up,
                                      -Vector3.right,
                                      -Vector3.forward);
                }

                /*bool t = y > 0 && level[x,y-1].Height > current.Height;
                
                bool r = x < mx && level[x+1,y].Height > current.Height;
                bool b = y < my && level[x,y+1].Height > current.Height;*/
            }
        }

        meshProxy.ToMesh(mesh);
    }
}
