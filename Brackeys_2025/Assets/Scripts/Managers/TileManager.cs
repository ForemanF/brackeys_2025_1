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
    Subscription<RevealNearestFogEvent> reveal_nearest_fog_sub;
    Subscription<ResetTileEvent> reset_tile_sub;

    [SerializeField]
    GameObject highlight_object;

    [SerializeField]
    BiomeTuple[] tile_biomes;

    [SerializeField]
    List<TileMesh> randomizable_meshes;

    [SerializeField]
    HexMeshTuple[] tile_meshes;

    Dictionary<TileBiome, Material> tile_biomes_dict;
    Dictionary<TileMesh, Mesh> tile_meshes_dict;

    List<HexTile> path;

    HexTile previous_hex_tile_action = null;
    Card previous_card= null;

    // Placement Rules
    Dictionary<Building, List<Building>> adjacent_building_restrictions;

    Dictionary<Building, List<TileMesh>> building_mesh_restrictions;

    Dictionary<Building, TileBiome> building_biome_restrictions;

    [SerializeField]
    List<Building> buildings;
    [SerializeField]
    List<TileMesh> building_tile_meshes;

    public Building GetRandomBuilding(Building not_this_building) {
        Building building = buildings[Random.Range(0, buildings.Count)];

        while(building == not_this_building) { 
            building = buildings[Random.Range(0, buildings.Count)];
        }

        return building;
    }

    public TileMesh GetRandomTileMesh() { 
        TileMesh tile_mesh = randomizable_meshes[Random.Range(0, randomizable_meshes.Count)];

        return tile_mesh;
    }

    public TileBiome GetRandomBiome() { 
        if(Random.Range(0, 1f) < 0.5f) {
            return TileBiome.Winter;
        }
        else {
            return TileBiome.Desert;
        }
    }

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

        adjacent_building_restrictions = new Dictionary<Building, List<Building>>();
        building_mesh_restrictions = new Dictionary<Building, List<TileMesh>>();
        building_biome_restrictions = new Dictionary<Building, TileBiome>();

        foreach(Building building in buildings) {
            adjacent_building_restrictions[building] = new List<Building>();
            building_mesh_restrictions[building] = new List<TileMesh>();
            building_biome_restrictions[building] = TileBiome.Nothing;
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

    public bool DoesSatisfyAllRestrictions(TileMesh building_mesh, HexTile hex_tile) {
        Building building = null;

        for(int i = 0; i < building_tile_meshes.Count; ++i) { 
            if(building_mesh == building_tile_meshes[i]) { 
                building = buildings[i];
                break;
            }
        }

        if(building == null) {
            return true;
        }


        if(hex_tile.GetTileBiome() == building_biome_restrictions[building]) {
            return false;
        }

        // individual hex requirements
        foreach(TileMesh tile_mesh in building_mesh_restrictions[building]) { 
            if(hex_tile.GetTileMesh() == tile_mesh) {
                return false;
            }
        }

        // neighbor requirements
        List<Building> adj_building_types = adjacent_building_restrictions[building];
        foreach(Building adj_requirement in adj_building_types) {
            bool check = false;
            foreach(HexTile adj_hex in hex_tile.GetNeighbors()) {
                GameObject adj_object = adj_hex.GetObjectOnHex();
                if(adj_object == null) {
                    continue;
                }
                if(adj_object.TryGetComponent<Building>(out Building adj_building)) { 
                    if(adj_building == adj_requirement) {
                        check = true;
                        break;
                    }
                }
                else {
                    continue;
                }
            }
            if(check == false) {
                return false;
            }
        }

        return true;
    }

    public void AddAdjRestriction(Building building, Building adj_requirement) {
        adjacent_building_restrictions[building].Add(adj_requirement);
    }

    public void AddTileMeshRestriction(Building building, TileMesh mesh_restriction) {
        building_mesh_restrictions[building].Add(mesh_restriction);
    }

    public void SetBiomeRestriction(Building building, TileBiome biome_restriction) {
        building_biome_restrictions[building] = biome_restriction;
    }

    private void Start()
    {
        highlight_tile_sub = EventBus.Subscribe<HighlightTileEvent>(_OnHighlightTile);
        highlight_card_action_sub = EventBus.Subscribe<HighlightCardActionEvent>(_OnHighlightCardAction);
        take_card_action_sub = EventBus.Subscribe<TakeCardActionEvent>(_OnTakeCardAction);
        reveal_tile_sub = EventBus.Subscribe<RevealTileEvent>(_OnRevealTile);
        reveal_nearest_fog_sub = EventBus.Subscribe<RevealNearestFogEvent>(_OnRevealNearestFog);
        reset_tile_sub = EventBus.Subscribe<ResetTileEvent>(_OnResetTile);
    }

    void _OnRevealTile(RevealTileEvent e) {
        RevealTile(e.revealed_tile);
    }

    void _OnRevealNearestFog(RevealNearestFogEvent e) {
        Debug.Log("Revealing nearby Hex Tiles");
        for(int i = 0; i < e.amount; ++i) {
            RevealNearestFog(e.source_tile);
        }
    }

    void _OnResetTile(ResetTileEvent e) {
        Debug.Log("Resetting tile");
        e.hex_tile.EmptyHex();
        e.hex_tile.SetTileMesh(TileMesh.Grass, tile_meshes_dict[TileMesh.Grass]);
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

        if(card_action.GetCardType() == CardAction.CardType.Building) { 
            TileMesh tile_mesh = card_action.GetTileMesh();
            Mesh temp_mesh = tile_meshes_dict[tile_mesh];
            card_action.DisplayCardAction(e.hex_tile, temp_mesh);
        }

        bool is_valid = card_action.IsValidPlacement(e.hex_tile, this, card_action.GetTileMesh());

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

        bool is_valid = card_action.IsValidPlacement(e.hex_tile, this, card_action.GetTileMesh());

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

                // Legacy random tile allocation
                //// temp random tile type
                //if (Random.Range(0f, 1f) < 0.1f)
                //{
                //    if (hex_tile.GetCubeCoords() != Vector3Int.zero)
                //    {
                //        TileBiome new_biome = TileBiome.Winter;
                //        hex_tile.SetTileBiome(new_biome, tile_biomes_dict[new_biome]);
                //    }
                //}
                //if (Random.Range(0f, 1f) < 0.1f)
                //{
                //    if (hex_tile.GetCubeCoords() != Vector3Int.zero)
                //    {
                //        TileBiome new_biome = TileBiome.Desert;
                //        hex_tile.SetTileBiome(new_biome, tile_biomes_dict[new_biome]);
                //    }
                //}

                //if (Random.Range(0, 1f) < 0.35f) {
                //    TileMesh new_mesh = (TileMesh)Random.Range(0, (int)System.Enum.GetValues(typeof(TileMesh)).Cast<TileMesh>().Max());
                //    hex_tile.SetTileMesh(new_mesh, tile_meshes_dict[new_mesh]);
                //}
            }
        }
    }

    public void RandomizeRandomTile() {
        HexTile hex_tile = GetRandomValid();

        if(Random.Range(0, 1f) < 0.33f) {
            TileBiome tile_biome = GetRandomBiome();
            if(Random.Range(0, 1f) < 0.5f) {
                tile_biome = TileBiome.Basic;
            }
            hex_tile.SetTileBiome(tile_biome, tile_biomes_dict[tile_biome]);
        }

        TileMesh rand_mesh = randomizable_meshes[Random.Range(0, randomizable_meshes.Count)];

        hex_tile.SetTileMesh(rand_mesh, tile_meshes_dict[rand_mesh]);
    }

    public HexTile GetRandomValid() {
        int rand_int = Random.Range(0, hex_tiles.Length);

        while (hex_tiles[rand_int].GetTileState() != TileState.Empty) { 
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

    void RevealNearestFog(HexTile hex_tile) {
        List<HexTile> tiles = new List<HexTile>();

        List<HexTile> unexplored = new List<HexTile>();
        List<HexTile> explored = new List<HexTile>();

        unexplored.Add(hex_tile);

        while(unexplored.Count > 0) {

            HexTile cur_explored = unexplored[0];

            if (cur_explored.IsRevealed() == false)
            {
                cur_explored.Reveal();
                return;
            }

            unexplored.RemoveAt(0);
            explored.Add(cur_explored);

            foreach(HexTile neighbor in cur_explored.GetNeighbors()) { 
                if(!explored.Contains(neighbor)) {
                    unexplored.Add(neighbor);
                }
            }
        }

        Debug.Log("No more tiles to reveal");

    }
}












