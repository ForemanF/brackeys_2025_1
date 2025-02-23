using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


[System.Serializable] public class BuildingPrefab { public TileMesh tile_mesh; public GameObject prefab; }

public class GameManager : MonoBehaviour
{
    [SerializeField]
    int energy = 3;

    [SerializeField]
    int max_energy = 3;

    [SerializeField]
    TextMeshProUGUI cost_text;
    Color cost_text_color;

    [SerializeField]
    DeckManager deck_manager;

    [SerializeField]
    float time_between_flash = 0.1f;

    [SerializeField]
    int flash_times = 3;

    int turn_number = 0;

    Coroutine flashing_red_energy = null;

    Subscription<NextTurnEvent> next_turn_sub;
    Subscription<DestroyedFactionObjectEvent> destroyed_object_sub;
    Subscription<CreateBuildingEvent> create_building_sub;

    bool is_ready_for_next_turn = true;

    [SerializeField]
    TileManager tile_manager;

    [SerializeField]
    GameObject enemy_prefab;

    [SerializeField]
    GameObject player_unit_prefab;

    [SerializeField]
    List<Unit> enemies;

    [SerializeField]
    List<Unit> player_units;

    [SerializeField]
    List<Building> player_buildings;

    [SerializeField]
    List<Building> enemy_buildings;

    [SerializeField]
    List<BuildingPrefab> building_prefab_list;

    Dictionary<TileMesh, GameObject> tile_mesh_to_prefab_dict; 

    // Start is called before the first frame update
    void Start()
    {
        enemies = new List<Unit>();
        player_units = new List<Unit>();

        tile_mesh_to_prefab_dict = new Dictionary<TileMesh, GameObject>();
        foreach(BuildingPrefab building_prefab in building_prefab_list) {
            tile_mesh_to_prefab_dict[building_prefab.tile_mesh] = building_prefab.prefab;
        }

        next_turn_sub = EventBus.Subscribe<NextTurnEvent>(_OnNextTurn);
        destroyed_object_sub = EventBus.Subscribe<DestroyedFactionObjectEvent>(_OnHandleDestroyedObject);
        create_building_sub = EventBus.Subscribe<CreateBuildingEvent>(_OnCreateBuilding);

        cost_text_color = cost_text.color;
        UpdateCostVisual();

        tile_manager.RevealTile(tile_manager.GetCenterTile());

        SpawnEnemy();
        SpawnEnemy();
        SpawnEnemy();
        //SpawnPlayerUnit();
    }

    public bool IsReadyForNextTurn() {
        return is_ready_for_next_turn;
    }

    void _OnNextTurn(NextTurnEvent e) {
        turn_number++;

        StartCoroutine(ProcessTurn());
    }

    void _OnHandleDestroyedObject(DestroyedFactionObjectEvent e) {
        if(e.game_object.TryGetComponent<Unit>(out Unit unit)) {
            RemoveUnitFromList(e.faction, unit);
        }
        else if(e.game_object.TryGetComponent<Building>(out Building building)) { 
            RemoveBuildingFromList(e.faction, building);
        
        }
        else {
            Debug.Log("No clean way to remove object");
        }
    }

    void _OnCreateBuilding(CreateBuildingEvent e) {
        Debug.Log("Creating Building Object");
        GameObject new_building_obj = Instantiate(tile_mesh_to_prefab_dict[e.building_card.GetTileMesh()]);

        Building building = new_building_obj.GetComponent<Building>();

        building.SetHex(e.hex_tile);

        new_building_obj.transform.position = e.hex_tile.transform.position;

        new_building_obj.GetComponent<HasHealth>().SetHealth(e.building_card.GetHealth());

        player_buildings.Add(building);

    }

    void RemoveUnitFromList(Faction faction, Unit unit) {
        List<Unit> remove_list;
        if(Faction.Player == faction) {
            remove_list = player_units;
        }
        else {
            remove_list = enemies;
        }

        remove_list.Remove(unit);
    }

    void RemoveBuildingFromList(Faction faction, Building building) {
        List<Building> remove_list;
        if(Faction.Player == faction) {
            remove_list = player_buildings;
        }
        else {
            remove_list = enemy_buildings;
        }

        EventBus.Publish(new ResetTileEvent(building.GetHex()));

        remove_list.Remove(building);
    }

    IEnumerator ProcessUnitActions(List<Unit> units, Faction faction) {
        foreach(Unit unit in units) {
            // set new targets for the units to go to the closest opposer
            HexTile new_target = FindNearestOpposingFaction(unit.GetHex(), faction);

            if(new_target != null) { 
                unit.SetPath(new_target);
            }
            else if(unit.HasTarget() == false) { 
                unit.SetPath(tile_manager.GetRandomValid());
            }

            yield return unit.ProcessTurn();
        }

        yield return null;
    }

    IEnumerator ProcessBuildingAttacks(List<Building> buildings, Faction faction) {
        foreach(Building building in buildings) {
            // set new targets for the units to go to the closest opposer
            HexTile nearest_enemy = FindNearestOpposingFaction(building.GetHex(), faction);

            yield return building.ProcessBuildingAttack(nearest_enemy);
        }

        yield return null;
    }

    IEnumerator ProcessTurn() {
        is_ready_for_next_turn = false;

        // my buildings attack
        yield return ProcessBuildingAttacks(player_buildings, Faction.Player);

        // my units move/attack
        yield return ProcessUnitActions(player_units, Faction.Player);

        // enemy units move/attack
        yield return ProcessUnitActions(enemies, Faction.Enemy);

        // event is processed
        // - card selection phase for new card
        // - boon selection

        // my buildings do any active abilities
        // - mine generates resource
        // extra draw card from whatever
        // spawn minion from whatever

        // player draws a card
        yield return deck_manager.DrawCards(1);
        EnergyChange(1);

        is_ready_for_next_turn = true;
    }

    public bool HasEnoughEnergy(int amount) { 
        if(energy - amount >= 0) {
            return true;
        }

        Debug.Log("Flashing the text!");
        if(flashing_red_energy != null) {
            StopCoroutine(flashing_red_energy);
        }

        flashing_red_energy = StartCoroutine(FlashLowEnergy());

        return false;
    }

    IEnumerator FlashLowEnergy() {

        for (int i = 0; i < flash_times; ++i) {
            cost_text.color = Color.red;
            yield return new WaitForSeconds(time_between_flash);

            cost_text.color = cost_text_color;
            yield return new WaitForSeconds(time_between_flash);
        }
    
    }

    public void EnergyChange(int amount) {
        energy += amount;
        energy = Mathf.Clamp(energy, 0, max_energy);

        UpdateCostVisual();
    }

    void UpdateCostVisual() {
        cost_text.text = energy.ToString() + "/" + max_energy.ToString();
    }

    void SpawnEnemy() {
        GameObject new_enemy = Instantiate(enemy_prefab);
        Unit enemy_unit = new_enemy.GetComponent<Unit>();

        HexTile random_tile = tile_manager.GetRandomValid();
        enemy_unit.SetPosition(random_tile, new Vector3(0, 0.65f, 0));

        random_tile = tile_manager.GetRandomValid();
        enemy_unit.SetPath(random_tile);

        enemies.Add(enemy_unit);
    }

    void SpawnPlayerUnit() {
        GameObject new_player_unit = Instantiate(player_unit_prefab);
        Unit player_unit = new_player_unit.GetComponent<Unit>();

        HexTile random_tile = tile_manager.GetRandomValid();
        player_unit.SetPosition(random_tile, new Vector3(0, 0.3f, 0));

        random_tile = tile_manager.GetRandomValid();
        player_unit.SetPath(random_tile);

        player_units.Add(player_unit);
    }

    HexTile FindNearestOpposingFaction(HexTile my_hex, Faction my_faction) {
        float min_dist = Mathf.Infinity;

        HexTile closest_opposing_tile = null;

        // CHECK NEARBY UNITS
        List<Unit> opposing_faction_units;
        if(my_faction == Faction.Player) {
            opposing_faction_units = enemies;
        }
        else {
            opposing_faction_units = player_units;
        }
        
        foreach(Unit opposing_faction in opposing_faction_units) {
            Vector3Int opposing_cube_coords = opposing_faction.GetHex().GetCubeCoords();
            Vector3Int my_cube_coords = my_hex.GetCubeCoords();
            float distance = Utilities.CubeDistance(opposing_cube_coords, my_cube_coords);

            if(distance < min_dist) {
                closest_opposing_tile = opposing_faction.GetHex();
                min_dist = distance;
            }
        }

        // CHECK NEARBY BUILDINGS
        List<Building> opposing_faction_buildings;
        if(my_faction == Faction.Player) {
            opposing_faction_buildings = enemy_buildings;
        }
        else {
            opposing_faction_buildings = player_buildings;
        }
        
        foreach(Building opposing_faction in opposing_faction_buildings) {
            Vector3Int opposing_cube_coords = opposing_faction.GetHex().GetCubeCoords();
            Vector3Int my_cube_coords = my_hex.GetCubeCoords();
            float distance = Utilities.CubeDistance(opposing_cube_coords, my_cube_coords);

            if(distance < min_dist) {
                closest_opposing_tile = opposing_faction.GetHex();
                min_dist = distance;
            }
        }

        return closest_opposing_tile;
    }

}
