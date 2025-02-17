using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType { 
    Default,
    Water
}

public class HexTile : MonoBehaviour
{
    [SerializeField]
    TileType tile_type = TileType.Default;

    [SerializeField]
    Vector2Int offset_coords = Vector2Int.zero;

    [SerializeField]
    Vector3Int cube_coords = Vector3Int.zero;

    List<HexTile> neighbors = null;

    [SerializeField]
    Material water_material;

    [SerializeField]
    MeshRenderer mesh_renderer;

    GameObject fow = null;

    public void SetOffsetCoords(int r, int c) {
        offset_coords = new Vector2Int(r, c);
    }

    public Vector2Int GetOffsetCoords() {
        return offset_coords;
    }

    public void SetCubeCoords(Vector2Int offset) {
        cube_coords = Utilities.OffsetToCube(offset);
    }

    public Vector3Int GetCubeCoords() {
        return cube_coords;
    }

    public void SetNeighbors(List<HexTile> new_neighbors) {
        neighbors = new_neighbors;
    }

    public List<HexTile> GetNeighbors() {
        return neighbors;
    }

    public TileType GetTileType() {
        return tile_type;
    }

    public void HighlightHex() {
        transform.localScale = 1.1f * Vector3.one;
    }

    public void SetTileType(TileType new_tile_type) {
        tile_type = new_tile_type;
        if(new_tile_type == TileType.Water) {
            mesh_renderer.material = water_material;
        }
    }

    public void SetFow(GameObject obj) {
        fow = obj;
        Utilities.SetLayerAndChildren(obj.transform, "Default");
    }

    public void Reveal() { 
        Utilities.SetLayerAndChildren(transform, "Default");

        if(fow == null) {
            return;
        }

        fow.SetActive(false);
    }

    public bool IsRevealed() {
        if(fow == null) { 
            return true;
        }

        return !fow.activeSelf;
    }

    private void OnDrawGizmos()
    {
        if(neighbors == null) {
            return;
        }

        foreach(HexTile neighbor in neighbors) {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }

}
