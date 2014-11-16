using System.Collections.Generic;
using UnityEngine;

public class MeshProxy
{
    public List<Vector3> verts = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<int> tris = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();
    
    public void AddQuad(Sprite[] tiles, int ind, Vector3 pos, Vector3 forward, Vector3 right, Vector3 up)
    {
        if (ind >= tiles.Length)
            return;

        tris.Add(verts.Count);
        tris.Add(verts.Count+1);
        tris.Add(verts.Count+2);
        
        tris.Add(verts.Count);
        tris.Add(verts.Count+2);
        tris.Add(verts.Count+3);

        verts.Add(pos);
        verts.Add(pos+right);
        verts.Add(pos+forward+right);
        verts.Add(pos+forward);

        normals.Add(up);
        normals.Add(up);
        normals.Add(up);
        normals.Add(up);
        
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