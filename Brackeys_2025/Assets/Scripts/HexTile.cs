using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TileBiome { 
    Basic,
    Winter,
    Desert
}

public enum TileState { 
    Default,
    Building
}

public enum TileMesh { 
    Grass,
    GrassForest,
    GrassHill,
    Stone,
    Sand,
    Water,
    Dirt,
    Tower,
    Hut,
    Mine
}

public class HexTile : MonoBehaviour
{
    [SerializeField]
    TileBiome tile_biome = TileBiome.Basic;

    [SerializeField]
    TileState tile_state = TileState.Default;

    [SerializeField]
    TileMesh tile_mesh = TileMesh.Grass;

    [SerializeField]
    MeshRenderer mesh_renderer;

    [SerializeField]
    MeshFilter mesh_filter;

    [SerializeField]
    Vector2Int offset_coords = Vector2Int.zero;

    [SerializeField]
    Vector3Int cube_coords = Vector3Int.zero;

    List<HexTile> neighbors = null;

    GameObject fow = null;

    Mesh my_actual_mesh;

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

    public TileBiome GetTileBiome() {
        return tile_biome;
    }

    public TileState GetTileState() {
        return tile_state;
    }

    public TileMesh GetTileMesh() {
        return tile_mesh;
    }

    public void SetTileState(TileState new_tile_state) {
        tile_state = new_tile_state;
    }

    public void HighlightHex() {
        transform.localScale = 1.1f * Vector3.one;
    }

    public void SetTileBiome(TileBiome new_biome, Material colormap) {
        tile_biome = new_biome;
        mesh_renderer.material = colormap;
    }

    public void SetTileMesh(TileMesh new_tile_mesh, Mesh new_mesh) {
        tile_mesh = new_tile_mesh;
        mesh_filter.mesh = new_mesh;
        my_actual_mesh = new_mesh;
    }

    public void SetTileMesh(HexMeshTuple hex_mesh_tuple) {
        SetTileMesh(hex_mesh_tuple.tile_mesh, hex_mesh_tuple.mesh);
    }

    public void SetTemporaryMesh(Mesh temp_mesh) {
        mesh_filter.mesh = temp_mesh;
    }

    public void RefreshTile() {
        mesh_filter.mesh = my_actual_mesh;
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
