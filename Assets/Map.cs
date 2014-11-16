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
    public Sprite[] Tiles;

	void Update() 
	{
        if (Tiles == null || Tiles.Length == 0)
            return;

        TileInfo[,] level = MakeLevel(30);//ParseLevel(Level.text);

        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = Tiles[0].texture;

        if (GetComponent<MeshFilter>().sharedMesh == null)
            GetComponent<MeshFilter>().sharedMesh = new Mesh();
        GenerateMesh(GetComponent<MeshFilter>().sharedMesh, level, Tiles);
	}

    public Vector3 Scale = Vector3.one;

    TileInfo[,] MakeLevel(int width)
    {
        TileInfo[,] level = new TileInfo[width,width];

        for (int x = 0; x<width; x++)
        {
            for (int y = 0; y<width; y++)
            {
                level[x,y].Height = (int)(Mathf.PerlinNoise(x*Scale.x,y*Scale.y) * Scale.z);
                level[x,y].Type = Tile.Floor;
            }
        }

        return level;
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
                for (int c = current.Height; c>=0; c--)
                {
                    int sideInd = 32;
                    if (c > 1) sideInd = 31;
                    if (c > 2) sideInd = 30;

                    if (c > (x == 0 ? 0 : level[x-1,y].Height))
                    {
                        meshProxy.AddQuad(tiles, sideInd,
                                      new Vector3(x,c-1,-y-1),
                                      Vector3.up,
                                      Vector3.forward,
                                      -Vector3.right);
                    }
                    if (c > (x == mx ? 0 : level[x+1,y].Height))
                    {
                        meshProxy.AddQuad(tiles, sideInd,
                                          new Vector3(x+1,c-1,-y),
                                          Vector3.up,
                                          -Vector3.forward,
                                          Vector3.right);
                    }
                    if (c > (y == 0 ? 0 : level[x,y-1].Height))
                    {
                        meshProxy.AddQuad(tiles, sideInd,
                                          new Vector3(x,c-1,-y),
                                          Vector3.up,
                                          Vector3.right,
                                          Vector3.forward);
                    }
                    if (c > (y == my ? 0 : level[x,y+1].Height))
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
