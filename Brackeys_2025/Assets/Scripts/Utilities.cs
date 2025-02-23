using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static Vector3Int OffsetToCube(Vector2Int offset_coords) {
        var q = offset_coords.y - (offset_coords.x - offset_coords.x % 2) / 2;
        if(offset_coords.x < 0) { 
            q = offset_coords.y - (offset_coords.x + offset_coords.x % 2) / 2;
        }
        var r = offset_coords.x;
        return new Vector3Int(q, r, -q - r);
    }

    public static Vector2Int CubeToOffset(Vector3Int cube_coords) {
        var col = cube_coords.x + (cube_coords.y - cube_coords.y % 2) / 2;
        if (cube_coords.y < 0)
        {
            col = cube_coords.x + (cube_coords.y + cube_coords.y % 2) / 2;
        }
        var row = cube_coords.y;
        return new Vector2Int(row, col);
    }

    public static void SetLayerAndChildren(Transform tf, string layer_name) {
        int layer = LayerMask.NameToLayer(layer_name);

        tf.gameObject.layer = layer;

        var children = tf.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }

    public static Vector3Int CubeSubtract(Vector3Int a, Vector3Int b) {
        return new Vector3Int(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static float CubeDistance(Vector3Int a, Vector3Int b) {
        Vector3Int vec = CubeSubtract(a, b);
        return (Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z)) / 2.0f;
    }
}
