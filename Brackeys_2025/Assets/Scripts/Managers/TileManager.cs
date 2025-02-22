using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable] public class BiomeTuple { public TileBiome biome; public Material colormap; }
[System.Serializable] public class HexMeshTuple { public TileMesh tile_mesh; public Mesh mesh; }

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

    [SerializeField]
    GameManager game_manager;

    Dictionary<Vector3Int, HexTile> tile_dict;
    HexTile[] hex_tiles = null;

    Subscription<HighlightTileEvent> highlight_tile_sub;
    Subscription<HighlightCardActionEvent> highlight_card_action_sub;
    Subscription<TakeCardActionEvent> take_card_action_sub;
    Subscription<RevealTileEvent> reveal_tile_sub;

    [SerializeField]
    GameObject highlight_object;

    [SerializeField]
    BiomeTuple[] tile_biomes;

    [SerializeField]
    HexMeshTuple[] tile_meshes;

    Dictionary<TileBiome, Material> tile_biomes_dict;
    Dictionary<TileMesh, Mesh> tile_meshes_dict;

    List<HexTile> path;

    HexTile previous_hex_tile_action = null;
    Card previous_card= null;

    private void Awake()
    {
        tile_biomes_dict = new Dictionary<TileBiome, Material>();
        foreach(BiomeTuple biome_tuple in tile_biomes) {
            tile_biomes_dict[biome_tuple.biome] = biome_tuple.colormap;
        }

        tile_meshes_dict = new Dictionary<TileMesh, Mesh>();
        foreach(HexMeshTuple hex_mesh_tuple in tile_meshes) {
            tile_meshes_dict[hex_mesh_tuple.tile_mesh] = hex_mesh_tuple.mesh;
        }

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
        highlight_tile_sub = EventBus.Subscribe<HighlightTileEvent>(_OnHighlightTile);
        highlight_card_action_sub = EventBus.Subscribe<HighlightCardActionEvent>(_OnHighlightCardAction);
        take_card_action_sub = EventBus.Subscribe<TakeCardActionEvent>(_OnTakeCardAction);
        reveal_tile_sub = EventBus.Subscribe<RevealTileEvent>(_OnRevealTile);
    }

    void _OnRevealTile(RevealTileEvent e) {
        RevealTile(e.revealed_tile);
    }

    void _OnHighlightTile(HighlightTileEvent e) {
        highlight_object.transform.position = e.hex_tile.transform.position;

        if(previous_hex_tile_action == null && previous_card == null) { 
            highlight_object.GetComponent<OpacityFader>().SetColor(Color.white);
        }
    }

    void _OnHighlightCardAction(HighlightCardActionEvent e) {
        if(previous_card == e.card && previous_hex_tile_action == e.hex_tile) {
            return;
        }

        if(previous_hex_tile_action != null && previous_hex_tile_action != e.hex_tile) {
            previous_hex_tile_action.RefreshTile();
        }

        // Handle the card action
        CardAction card_action = e.card.GetCardAction();

        TileMesh tile_mesh = card_action.GetTileMesh();
        Mesh temp_mesh = tile_meshes_dict[tile_mesh];

        bool is_valid = card_action.DisplayCardAction(e.hex_tile, temp_mesh);

        if(is_valid) { 
            highlight_object.GetComponent<OpacityFader>().SetColor(Color.green);
        }
        else { 
            highlight_object.GetComponent<OpacityFader>().SetColor(Color.red);
        }

        previous_hex_tile_action = e.hex_tile;
        previous_card = e.card;
    }

    void _OnTakeCardAction(TakeCardActionEvent e) { 
        CardAction card_action = e.card.GetCardAction();

        bool is_valid = card_action.IsValidPlacement(e.hex_tile);

        if(is_valid == false) {
            e.card.GoToBasePosition();

            if(previous_hex_tile_action != null) { 
                previous_hex_tile_action.RefreshTile();
            }

            previous_hex_tile_action = null;
            previous_card = null;
        }
        else {
            // put the card into the discard pile
            EventBus.Publish(new DiscardCardEvent(e.card));

            // NEED TO PAY THE CARD COST
            int amount = -card_action.GetCost();
            game_manager.EnergyChange(amount);

            // process the card action
            TileMesh tile_mesh = card_action.GetTileMesh();
            HexMeshTuple hex_mesh_tuple = new HexMeshTuple();
            hex_mesh_tuple.tile_mesh = tile_mesh;
            hex_mesh_tuple.mesh = tile_meshes_dict[tile_mesh];
            card_action.PerformCardAction(e.hex_tile, hex_mesh_tuple);
        }
        highlight_object.GetComponent<OpacityFader>().SetColor(Color.white);
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

                // default all tiles to grass
                hex_tile.SetTileMesh(TileMesh.Grass, tile_meshes_dict[TileMesh.Grass]);

                // temp random tile type
                if (Random.Range(0f, 1f) < 0.1f)
                {
                    if (hex_tile.GetCubeCoords() != Vector3Int.zero)
                    {
                        TileBiome new_biome = TileBiome.Winter;
                        hex_tile.SetTileBiome(new_biome, tile_biomes_dict[new_biome]);
                    }
                }
                if (Random.Range(0f, 1f) < 0.1f)
                {
                    if (hex_tile.GetCubeCoords() != Vector3Int.zero)
                    {
                        TileBiome new_biome = TileBiome.Desert;
                        hex_tile.SetTileBiome(new_biome, tile_biomes_dict[new_biome]);
                    }
                }

                if (Random.Range(0, 1f) < 0.35f) {
                    TileMesh new_mesh = (TileMesh)Random.Range(0, (int)System.Enum.GetValues(typeof(TileMesh)).Cast<TileMesh>().Max());
                    hex_tile.SetTileMesh(new_mesh, tile_meshes_dict[new_mesh]);
                }
            }
        }
    }

    public HexTile GetRandomValid() {
        int rand_int = Random.Range(0, hex_tiles.Length);

        while (hex_tiles[rand_int].GetTileState() != TileState.Default) { 
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
