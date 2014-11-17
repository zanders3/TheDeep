using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(Map))]
public class MapInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Map map = target as Map;

        EditorGUI.BeginChangeCheck();
        Texture2D spritesheet = (Texture2D)EditorGUILayout.ObjectField("Spritesheet", map.Tiles != null && map.Tiles.Length > 0 ? map.Tiles[0].texture : null, typeof(Texture2D), false);
        if (EditorGUI.EndChangeCheck())
        {
            map.Tiles = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(spritesheet))
                .Where(asset => asset is Sprite)
                .Select(asset => (Sprite)asset)
                .ToArray();
        }

        base.OnInspectorGUI();
    }
}

