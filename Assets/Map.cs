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

        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
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
        for (int x = 0; x<width; x++)
        {
            for (int y = 0; y<height; y++)
            {
                tiles[x,y] = (Tile)((int)lines[y][x] - 48);
            }
        }

        return tiles;
    }

    static int GetTileIndex(int x, int y, Tile[,] level)
    {
        int idx = 0;
        Tile current = level[x,y];

        if (y > 0 && level[x,y-1] == current) idx += 4;
        if (y < level.GetLength(1)-1 && level[x,y+1] == current) idx += 1;

        if (x > 0 && level[x-1,y] == current) idx += 8;
        if (x < level.GetLength(0)-1 && level[x+1,y] == current) idx += 2;

        return idx;
    }

    static void GenerateMesh(Mesh mesh, Tile[,] level, Sprite[] tiles)
    {
        Dictionary<string, List<Sprite>> spriteMap = tiles
            .GroupBy(tile => tile.name)
            .Select(group => group.ToList())
            .ToDictionary(group => group[0].name, group => group);

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        for (int x = 0; x<level.GetLength(0); x++)
        {
            for (int y = 0; y<level.GetLength(1); y++)
            {
                if (level[x,y] == Tile.Empty)
                    continue;

                tris.Add(verts.Count);
                tris.Add(verts.Count+1);
                tris.Add(verts.Count+2);

                tris.Add(verts.Count);
                tris.Add(verts.Count+2);
                tris.Add(verts.Count+3);

                verts.Add(new Vector3(x,   -y));
                verts.Add(new Vector3(x+1, -y));
                verts.Add(new Vector3(x+1, -y-1));
                verts.Add(new Vector3(x,   -y-1));

                int idx = GetTileIndex(x,y,level);
                string key = level[x,y].ToString().ToLower() + "_" + idx;
                Debug.Log(key);
                List<Sprite> matchingTiles = spriteMap[key];

                Rect rect = matchingTiles[Random.Range(0,matchingTiles.Count)].rect;
                float xmin = rect.xMin / tiles[0].texture.width, ymin = rect.yMin / tiles[0].texture.height;
                float xmax = rect.xMax / tiles[0].texture.width, ymax = rect.yMax / tiles[0].texture.height;

                uvs.Add(new Vector2(xmin, ymin));
                uvs.Add(new Vector2(xmax, ymin));
                uvs.Add(new Vector2(xmax, ymax));
                uvs.Add(new Vector2(xmin, ymax));
            }
        }

        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
    }
}
