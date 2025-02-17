using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField]
    int width = 15;

    [SerializeField]
    int height = 15;

    [SerializeField]
    GameObject hex_tile_pf;

    [SerializeField]
    bool enable_fog = false;

    [SerializeField]
    GameObject fow_pf;

    Dictionary<Vector3Int, HexTile> tile_dict;
    HexTile[] hex_tiles = null;

    Subscription<HighlightTileEvent> highlight_sub;

    [SerializeField]
    GameObject highlight_object;

    List<HexTile> path;

    private void Awake()
    {
        CreateGrid();

        tile_dict = new Dictionary<Vector3Int, HexTile>();

        hex_tiles = gameObject.GetComponentsInChildren<HexTile>();
        foreach(HexTile hex_tile in hex_tiles) {
            RegisterTile(hex_tile);

            if(enable_fog) { 
                AddFogOfWar(hex_tile);
            }
        }

        foreach(HexTile hex_tile in hex_tiles) {
            List<HexTile> neighbors = GetNeighbors(hex_tile);
            hex_tile.SetNeighbors(neighbors);
        }
    }

    private void Start()
    {
        highlight_sub = EventBus.Subscribe<HighlightTileEvent>(_OnHighlightTile);
    }

    void _OnHighlightTile(HighlightTileEvent e) {
        highlight_object.transform.position = e.hex_tile.transform.position;
    }

    List<HexTile> GetNeighbors(HexTile hex_tile) {
        List<HexTile> neighbors = new List<HexTile>();

        Vector3Int[] neighbor_coords = new Vector3Int[] {
            new Vector3Int(1, -1, 0),
            new Vector3Int(1, 0, -1),
            new Vector3Int(0, 1, -1),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(0, -1, 1)
        };

        foreach(Vector3Int neighbor_coord in neighbor_coords) {
            Vector3Int tile_coord = hex_tile.GetCubeCoords();

            if(tile_dict.TryGetValue(tile_coord + neighbor_coord, out HexTile neighbor)) {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    void RegisterTile(HexTile hex_tile) {
        tile_dict.Add(hex_tile.GetCubeCoords(), hex_tile);
    }

    void CreateGrid()
    {
        float tile_size = hex_tile_pf.transform.localScale.x * 1f;
        float tile_width = tile_size * 2;
        float tile_height = Mathf.Sqrt(3);

        Vector3 new_tile_location = new Vector3(-height / 2 * tile_height, 0, -width / 2 * tile_width);
        for(int r = 0; r < height; r++) { 
            new_tile_location += new Vector3(tile_height, 0, 0);
            new_tile_location.z = -width / 2 * tile_width;
            if(r % 2 == 0) { 
                new_tile_location.z += tile_width / 2;
            }
            for(int c = 0; c < width; c++) {
                new_tile_location += new Vector3(0, 0, tile_width);

                GameObject new_tile = Instantiate(hex_tile_pf, new_tile_location, Quaternion.identity, transform);
                HexTile hex_tile = new_tile.GetComponent<HexTile>();
                hex_tile.SetOffsetCoords(r - height / 2, c - width / 2);
                Vector2Int offset_coords = hex_tile.GetOffsetCoords();
                hex_tile.SetCubeCoords(offset_coords);

                // temp random tile type
                if(Random.Range(0f, 1f) < 0.1f) {
                    if(hex_tile.GetCubeCoords() != Vector3Int.zero) { 
                        hex_tile.SetTileType(TileType.Water);
                    }
                }

            }
        }
    }

    public HexTile GetRandom() {
        int rand_int = Random.Range(0, hex_tiles.Length);

        while (hex_tiles[rand_int].GetTileType() != TileType.Default) { 
            rand_int = Random.Range(0, hex_tiles.Length);
        }

        return hex_tiles[rand_int];
    }

    public HexTile GetCenterTile() {
        return tile_dict[Vector3Int.zero];
    }

    public void OnDrawGizmos() { 
        if(path != null) { 
            foreach(HexTile hex_tile in path) {
                Gizmos.DrawCube(hex_tile.transform.position + new Vector3(0, 0.5f, 0), Vector3.one * 0.5f);
            }
        }
    }

    void AddFogOfWar(HexTile hex_tile) {
        GameObject fow = Instantiate(fow_pf, hex_tile.transform);
        fow.name = "Fow " + hex_tile.GetCubeCoords();
        Utilities.SetLayerAndChildren(hex_tile.transform, "Hidden");
        hex_tile.SetFow(fow);
    }

    public void RevealTile(HexTile hex_tile) {
        hex_tile.Reveal();
        foreach(HexTile neighbor in hex_tile.GetNeighbors()) {
            neighbor.Reveal();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
