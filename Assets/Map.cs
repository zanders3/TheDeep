using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

enum Tile : int
{
    Empty = 0,
    Floor = 1,
    Water = 2
}

public class MeshProxy
{
	public List<Vector3> verts = new List<Vector3>();
	public List<int> tris = new List<int>();
	public List<Vector2> uvs = new List<Vector2>();

	public void AddQuad(Sprite[] tiles, int ind, int x, int y, float z)
	{
		tris.Add(verts.Count);
		tris.Add(verts.Count+1);
		tris.Add(verts.Count+2);
		
		tris.Add(verts.Count);
		tris.Add(verts.Count+2);
		tris.Add(verts.Count+3);
		
		verts.Add(new Vector3(x,   -y, z));
		verts.Add(new Vector3(x+1, -y, z));
		verts.Add(new Vector3(x+1, -y-1, z));
		verts.Add(new Vector3(x,   -y-1, z));

        Rect rect = tiles[ind].rect;
		float xmin = rect.xMin / tiles[0].texture.width, ymin = rect.yMin / tiles[0].texture.height;
		float xmax = rect.xMax / tiles[0].texture.width, ymax = rect.yMax / tiles[0].texture.height;
		
		uvs.Add(new Vector2(xmin, ymin));
		uvs.Add(new Vector2(xmax, ymin));
		uvs.Add(new Vector2(xmax, ymax));
		uvs.Add(new Vector2(xmin, ymax));
	}

	public void ToMesh(Mesh mesh)
	{
		mesh.Clear();
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = tris.ToArray();
	}
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

        Tile[,] level = ParseLevel(Level.text);

        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("TheDeep/Tile"));
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = Tiles[0].texture;

        if (GetComponent<MeshFilter>().sharedMesh == null)
            GetComponent<MeshFilter>().sharedMesh = new Mesh();
        GenerateMesh(GetComponent<MeshFilter>().sharedMesh, level, Tiles);
	}

    static Tile[,] ParseLevel(string level)
    {
        string[] lines = level.Split('\n');
        int width = lines[0].Length, height = lines.Length;

        Tile[,] tiles = new Tile[width,height];
		for (int y = 0; y<height; y++)
        {
			for (int x = 0; x<width&&x<lines[y].Length; x++)    
            {
				Tile tile = (Tile)((int)lines[y][x] - 48);
				tiles[x,y] = tile;
            }
        }

        return tiles;
    }

    static int GetEdgeIndex(int x, int y, Tile[,] level)
    {
        int idx = 0;
        Tile current = level[x,y];

        if (x <= 0 || level[x-1,y] != current) idx += 1;
        if (y >= level.GetLength(1)-1 || level[x,y+1] != current) idx += 2;
        if (x >= level.GetLength(0)-1 || level[x+1,y] != current) idx += 4;
        if (y <= 0 || level[x,y-1] != current) idx += 8;

        return idx;
    }

	static int GetCornerIndex(int x, int y, Tile[,] level)
	{
        int idx = 0;
        Tile current = level[x,y];

        if (y <= 0 || x <= 0 || y >= level.GetLength(1) - 1 || x >= level.GetLength(0) - 1)
            idx = 0;
        else
        {
            if (level[x-1,y+1] != current) idx += 1;
            if (level[x+1,y+1] != current) idx += 2;
            if (level[x+1,y-1] != current) idx += 4;
            if (level[x-1,y-1] != current) idx += 8;
        }

		return idx + 15;
	}

    static void GenerateMesh(Mesh mesh, Tile[,] level, Sprite[] tiles)
    {
		MeshProxy meshProxy = new MeshProxy();

        for (int x = 0; x<level.GetLength(0); x++)
        {
            for (int y = 0; y<level.GetLength(1); y++)
            {
                if (level[x,y] == Tile.Empty)
                    continue;

                meshProxy.AddQuad(tiles, GetEdgeIndex(x,y,level), x, y, 0.0f);
                meshProxy.AddQuad(tiles, GetCornerIndex(x,y,level), x, y, -0.1f);
            }
        }

        meshProxy.ToMesh(mesh);
    }
}
