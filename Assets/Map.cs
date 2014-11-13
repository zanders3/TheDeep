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
    public List<Vector3> normals = new List<Vector3>();
	public List<int> tris = new List<int>();
	public List<Vector2> uvs = new List<Vector2>();

	public void AddQuad(Sprite[] tiles, int ind, int x, int y, float z)
	{
        if (ind >= tiles.Length)
            return;

		tris.Add(verts.Count);
		tris.Add(verts.Count+1);
		tris.Add(verts.Count+2);
		
		tris.Add(verts.Count);
		tris.Add(verts.Count+2);
		tris.Add(verts.Count+3);
		
        verts.Add(new Vector3(x,  z, -y));
        verts.Add(new Vector3(x+1,z, -y));
        verts.Add(new Vector3(x+1,z, -y-1));
		verts.Add(new Vector3(x,  z, -y-1));

        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);

        Rect rect = tiles[ind].rect;
		float xmin = rect.xMin / tiles[0].texture.width, ymin = rect.yMin / tiles[0].texture.height;
		float xmax = rect.xMax / tiles[0].texture.width, ymax = rect.yMax / tiles[0].texture.height;
		
		uvs.Add(new Vector2(xmin, ymax));
		uvs.Add(new Vector2(xmax, ymax));
		uvs.Add(new Vector2(xmax, ymin));
		uvs.Add(new Vector2(xmin, ymin));
	}

	public void ToMesh(Mesh mesh)
	{
		mesh.Clear();
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
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

        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
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

    static int GetEdgeIndex(int x, int y, Tile[,] level, Tile current)
    {
        if (level[x,y] != current)
            return 0;

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

    static void GenerateMesh(Mesh mesh, Tile[,] level, Sprite[] tiles)
    {
		MeshProxy meshProxy = new MeshProxy();

        for (int x = 0; x<level.GetLength(0); x++)
        {
            for (int y = 0; y<level.GetLength(1); y++)
            {
                Tile current = level[x,y];
                if (current == Tile.Empty)
                    continue;

                int ind = GetEdgeIndex(x,y,level,level[x,y]);
                if (ind == 0) 
                    continue;
                if (current == Tile.Water) ind += 15;

                meshProxy.AddQuad(tiles, ind-1, x, y, 0.0f);
            }
        }

        meshProxy.ToMesh(mesh);
    }
}
