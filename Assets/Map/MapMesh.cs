
using UnityEngine;

public static class MapMesh
{
    static int GetEdgeIndex(int x, int y, TileInfo[,] level, TileInfo current)
    {
        int mx = level.GetLength(0) - 1, my = level.GetLength(1) - 1;
        
        bool l = x > 0 && level[x-1,y] >= current;
        bool t = y > 0 && level[x,y+1] >= current;
        
        bool r = x < mx && level[x+1,y] >= current;
        bool b = y < my && level[x,y-1] >= current;
        
        bool tl = x > 0  && y > 0  && level[x-1,y+1] >= current;
        bool tr = x < mx && y > 0  && level[x+1,y+1] >= current;
        bool bl = x > 0  && y < my && level[x-1,y-1] >= current;
        bool br = x < mx && y < my && level[x+1,y-1] >= current;
        
        int idx = 0;
        if (l && tl && t) idx += 1;
        if (t && tr && r) idx += 2;
        if (r && br && b) idx += 8;
        if (b && bl && l) idx += 4;
        
        return idx;
    }
    
    static int GetHeight(TileInfo tile)
    {
        switch (tile.Type)
        {
            case Tile.BridgeE:
            case Tile.BridgeN:
                return 0;
            case Tile.RampN:
            case Tile.RampE:
            case Tile.RampS:
            case Tile.RampW:
                return tile.Height - 1;
            default:
                return tile.Height;
        }
    }
    
    public static void GenerateMesh(Mesh mesh, TileInfo[,] level, Sprite[] tiles)
    {
        MeshProxy meshProxy = new MeshProxy();
        
        for (int x = 0; x<level.GetLength(0); x++)
        {
            for (int y = 0; y<level.GetLength(1); y++)
            {
                TileInfo current = level[x,y];
                if (current.Height == 0)
                    continue;

                switch (current.Type)
                {
                    case Tile.BridgeN:
                    case Tile.BridgeE:
                        meshProxy.AddQuad(tiles, current.Type == Tile.BridgeN ? 34 : 35,
                                          new Vector3(x-0.1f,current.Height+((x+y)*0.0001f),y+1.1f),
                                          -Vector3.forward*1.2f,
                                          Vector3.right*1.2f,
                                          Vector3.up);
                        continue;
                    case Tile.RampN:
                        meshProxy.AddQuad(tiles, 36,
                                          new Vector3(x,current.Height,y+1),
                                          Vector3.down-Vector3.forward,
                                          Vector3.right,
                                          Vector3.up);
                        continue;
                    case Tile.RampS:
                        meshProxy.AddQuad(tiles, 36,
                                          new Vector3(x,current.Height-1,y),
                                          -Vector3.down-Vector3.forward,
                                          Vector3.right,
                                          Vector3.up);
                        continue;
                    case Tile.RampE:
                        meshProxy.AddQuad(tiles, 36,
                                          new Vector3(x,current.Height,y),
                                          Vector3.right+Vector3.down,
                                          Vector3.forward,
                                          Vector3.up);
                        continue;
                    case Tile.RampW:
                        meshProxy.AddQuad(tiles, 36,
                                          new Vector3(x,current.Height-1,y),
                                          Vector3.right-Vector3.down,
                                          Vector3.forward,
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
                                  new Vector3(x,current.Height,y+1),
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
                                          new Vector3(x,c-1,y),
                                          Vector3.up,
                                          Vector3.forward,
                                          -Vector3.right);
                    }
                    if (c > (x == mx ? 0 : GetHeight(level[x+1,y])))
                    {
                        meshProxy.AddQuad(tiles, sideInd,
                                          new Vector3(x+1,c-1,y+1),
                                          Vector3.up,
                                          -Vector3.forward,
                                          Vector3.right);
                    }
                    if (c > (y == 0 ? 0 : GetHeight(level[x,y+1])))
                    {
                        meshProxy.AddQuad(tiles, sideInd,
                                          new Vector3(x,c-1,y+1),
                                          Vector3.up,
                                          Vector3.right,
                                          Vector3.forward);
                    }
                    if (c > (y == my ? 0 : GetHeight(level[x,y-1])))
                    {
                        meshProxy.AddQuad(tiles, sideInd,
                                          new Vector3(x+1,c-1,y),
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
