using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    [SerializeField]
    GameObject game_over_screen;

    [SerializeField]
    TextMeshProUGUI turn_text;

    Coroutine flashing_red_energy = null;

    Subscription<NextTurnEvent> next_turn_sub;
    Subscription<DestroyedFactionObjectEvent> destroyed_object_sub;
    Subscription<CreateBuildingEvent> create_building_sub;
    Subscription<SpawnSoldierEvent> spawn_soldier_sub;
    Subscription<IncreaseEnergyEvent> increase_energy_sub;
    Subscription<DrawCardEvent> draw_card_sub;

    bool is_ready_for_next_turn = false;

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

    int cards_needed_to_draw = 0;

    int num_randomize_tiles = 2;

    float boon_lerp_speed_s = 1.2f;

    [SerializeField]
    RectTransform title;

    [SerializeField]
    TextMeshProUGUI title_text;
    Vector3 title_offscreen_pos;
    Vector3 title_onscreen_pos;

    [SerializeField]
    RectTransform option_1;
    Vector3 offscreen_pos_1;
    Vector3 onscreen_pos_1;

    [SerializeField]
    TextMeshProUGUI option_1_text;

    [SerializeField]
    RectTransform option_2;
    Vector3 offscreen_pos_2;
    Vector3 onscreen_pos_2;

    [SerializeField]
    TextMeshProUGUI option_2_text;

    [SerializeField]
    RectTransform option_3;

    [SerializeField]
    TextMeshProUGUI option_3_text;
    Vector3 offscreen_pos_3;
    Vector3 onscreen_pos_3;

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
        increase_energy_sub = EventBus.Subscribe<IncreaseEnergyEvent>(_OnIncreaseEnergy);
        spawn_soldier_sub = EventBus.Subscribe<SpawnSoldierEvent>(_OnSpawnSoldier);
        draw_card_sub = EventBus.Subscribe<DrawCardEvent>(_OnDrawCard);

        cost_text_color = cost_text.color;
        UpdateCostVisual();

        tile_manager.RevealTile(tile_manager.GetCenterTile());

        title_onscreen_pos = title.anchoredPosition;
        title_offscreen_pos = title_onscreen_pos + new Vector3(0, 400, 0);

        onscreen_pos_1 = option_1.anchoredPosition;
        offscreen_pos_1 = onscreen_pos_1 + new Vector3(0, -800, 0);

        onscreen_pos_2 = option_2.anchoredPosition;
        offscreen_pos_2 = onscreen_pos_2 + new Vector3(-600, 0, 0);

        onscreen_pos_3 = option_3.anchoredPosition;
        offscreen_pos_3 = onscreen_pos_3 + new Vector3(600, 0, 0);

        title.anchoredPosition = title_offscreen_pos;
        option_1.anchoredPosition = offscreen_pos_1;
        option_2.anchoredPosition = offscreen_pos_2;
        option_3.anchoredPosition = offscreen_pos_3;

        SpawnEnemy();

        StartCoroutine(StartingLogic());
    }

    IEnumerator StartingLogic() {
        yield return new WaitForSeconds(5);

        yield return deck_manager.BringStrongholdToHand();

        is_ready_for_next_turn = true;
    }

    public bool IsReadyForNextTurn() {
        return is_ready_for_next_turn;
    }

    void _OnNextTurn(NextTurnEvent e) {
        turn_number++;

        turn_text.text = "Text " + turn_number.ToString();

        StartCoroutine(ProcessTurn());
    }

    void _OnIncreaseEnergy(IncreaseEnergyEvent e) {
        EnergyChange(e.energy_increase);
    }

    void _OnSpawnSoldier(SpawnSoldierEvent e) {
        SpawnPlayerUnit(e.soldier_prefab, e.location);
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
        GameObject new_building_obj = Instantiate(tile_mesh_to_prefab_dict[e.building_card.GetTileMesh()]);

        Building building = new_building_obj.GetComponent<Building>();

        building.SetHex(e.hex_tile);

        building.SetSpecialValue(e.building_card.GetSpecialValue());
        building.SetStrength(e.building_card.GetStrength());

        new_building_obj.transform.position = e.hex_tile.transform.position;

        new_building_obj.GetComponent<HasHealth>().SetHealth(e.building_card.GetHealth());

        tile_manager.RevealTile(e.hex_tile);

        if(new_building_obj.TryGetComponent<Hut>(out Hut hut)) {
            IncreaseMaxEnergy(e.building_card.GetSpecialValue());
            deck_manager.IncreaseMaxHandSize(e.building_card.GetSpecialValue());
        }

        player_buildings.Add(building);
    }

    void _OnDrawCard(DrawCardEvent e) {
        cards_needed_to_draw += e.amount;
        Debug.Log(cards_needed_to_draw);
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

        if(building.TryGetComponent<Hut>(out Hut hut)) { 
            IncreaseMaxEnergy(-building.GetSpecialValue());
            deck_manager.IncreaseMaxHandSize(-building.GetSpecialValue());
        }

        //if(building.TryGetComponent<Stronghold>(out Stronghold strong)) {
        //    Debug.Log("You Lose");
        //    game_over_screen.SetActive(true);
        //}

        EventBus.Publish(new ResetTileEvent(building.GetHex()));

        remove_list.Remove(building);

        if (building.TryGetComponent<Stronghold>(out Stronghold strong))
        {
            Debug.Log("You Lose");
            game_over_screen.SetActive(true);
        }
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

    IEnumerator ProcessBuildingActions(List<Building> buildings, Faction faction) {
        foreach(Building building in buildings) {
            // set new targets for the units to go to the closest opposer
            HexTile nearest_enemy = FindNearestOpposingFaction(building.GetHex(), faction);

            yield return building.ProcessBuildingAction();
        }

        yield return null;
    }

    IEnumerator RectLerp(RectTransform rt, Vector3 start, Vector3 end, float total_time) {
        float progress = 0;
        float start_time = Time.time;

        while(progress < 1) {
            progress = (Time.time - start_time) / total_time;
            rt.anchoredPosition = Vector3.Lerp(start, end, progress);

            yield return null;
        }

        rt.anchoredPosition = end;
    }

    public void SelectOption(int option_num) {
        can_proceed = true;
        selected_option = option_num;
        
    }

    bool can_proceed = false;
    int selected_option = 1;

    IEnumerator ProcessBoon() {
        can_proceed = false;

        Building building_to_get_restricted1 = tile_manager.GetRandomBuilding(null);
        Building building_restriction1 = tile_manager.GetRandomBuilding(building_to_get_restricted1);
        TileMesh random_mesh1 = tile_manager.GetRandomTileMesh();
        TileBiome random_biome1 = tile_manager.GetRandomBiome();

        Building building_to_get_restricted2 = tile_manager.GetRandomBuilding(null);
        Building building_restriction2 = tile_manager.GetRandomBuilding(building_to_get_restricted2);
        TileMesh random_mesh2 = tile_manager.GetRandomTileMesh();
        TileBiome random_biome2 = tile_manager.GetRandomBiome();

        Building building_to_get_restricted3 = tile_manager.GetRandomBuilding(null);
        Building building_restriction3 = tile_manager.GetRandomBuilding(building_to_get_restricted3);
        TileMesh random_mesh3 = tile_manager.GetRandomTileMesh();
        TileBiome random_biome3 = tile_manager.GetRandomBiome();

        option_1_text.text = building_to_get_restricted1.name;
        option_2_text.text = building_to_get_restricted2.name;
        option_3_text.text = building_to_get_restricted3.name;

        if(turn_number % 24 == 0) {
            title_text.text = "Biome Tile Restriction";

            option_1_text.text += " can't be placed in the ";
            option_1_text.text += random_mesh1.ToString();
            option_1_text.text += " biome";

            option_2_text.text += " can't be placed in the ";
            option_2_text.text += random_mesh2.ToString();
            option_2_text.text += " biome";

            option_3_text.text += " can't be placed in the ";
            option_3_text.text += random_mesh3.ToString();
            option_3_text.text += " biome";
        }
        else if(turn_number % 32 == 0) { 
            title_text.text = "Adjacent Building Restriction";

            option_1_text.text += " must be placed next to ";
            option_1_text.text += building_restriction1;

            option_2_text.text += " must be placed next to ";
            option_2_text.text += building_restriction2;

            option_3_text.text += " must be placed next to ";
            option_3_text.text += building_restriction3;
        }
        else if(turn_number % 8 == 0) {
            title_text.text = "Terrain Tile Restriction";

            option_1_text.text += " can't be placed in the ";
            option_1_text.text += random_mesh1.ToString();
            option_1_text.text += " terrain";

            option_2_text.text += " can't be placed in the ";
            option_2_text.text += random_mesh2.ToString();
            option_2_text.text += " terrain";

            option_3_text.text += " can't be placed in the ";
            option_3_text.text += random_mesh3.ToString();
            option_3_text.text += " terrain";
        }
        // determine the options

        // set the text

        // Lerp the menus in
        StartCoroutine(RectLerp(title, title_offscreen_pos, title_onscreen_pos, boon_lerp_speed_s));
        StartCoroutine(RectLerp(option_1, offscreen_pos_1, onscreen_pos_1, boon_lerp_speed_s));
        StartCoroutine(RectLerp(option_2, offscreen_pos_2, onscreen_pos_2, boon_lerp_speed_s));
        StartCoroutine(RectLerp(option_3, offscreen_pos_3, onscreen_pos_3, boon_lerp_speed_s));


        while(can_proceed == false) {
            yield return null;
        }

        Building selected_restricted_building;
        Building selected_building;
        TileBiome selected_biome;
        TileMesh selected_mesh;

        if(selected_option == 1) {
            selected_restricted_building = building_to_get_restricted1;
            selected_building = building_restriction1;
            selected_biome = random_biome1;
            selected_mesh = random_mesh1;
        }
        else if(selected_option == 2) { 
            selected_restricted_building = building_to_get_restricted2;
            selected_building = building_restriction2;
            selected_biome = random_biome2;
            selected_mesh = random_mesh2;
        }
        else { 
            selected_restricted_building = building_to_get_restricted3;
            selected_building = building_restriction3;
            selected_biome = random_biome3;
            selected_mesh = random_mesh3;
        }

        if(turn_number % 24 == 0) {
            tile_manager.SetBiomeRestriction(selected_restricted_building, selected_biome);
        }
        else if(turn_number % 32 == 0) {
            tile_manager.AddAdjRestriction(selected_restricted_building, selected_building);
        }
        else if(turn_number % 8 == 0) {
            tile_manager.AddTileMeshRestriction(selected_restricted_building, selected_mesh);
        }

        StartCoroutine(RectLerp(title, title_onscreen_pos, title_offscreen_pos, boon_lerp_speed_s));
        StartCoroutine(RectLerp(option_1, onscreen_pos_1, offscreen_pos_1, boon_lerp_speed_s));
        StartCoroutine(RectLerp(option_2, onscreen_pos_2, offscreen_pos_2, boon_lerp_speed_s));
        StartCoroutine(RectLerp(option_3, onscreen_pos_3, offscreen_pos_3, boon_lerp_speed_s));

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
        if(turn_number % 8 == 0) { 
            yield return ProcessBoon();

            num_randomize_tiles += 1;
        }

        if(turn_number % 4 == 0) { 
            for(int i = 0; i < num_randomize_tiles + turn_number / 10; ++i) {
                SpawnEnemy();
            }
        
        }


        // randomize tiles
        for(int i = 0; i < num_randomize_tiles; ++i) {
            tile_manager.RandomizeRandomTile();
        }

        // my buildings do any active abilities
        yield return ProcessBuildingActions(player_buildings, Faction.Player);

        // player draws a card
        yield return deck_manager.DrawCards(cards_needed_to_draw);
        cards_needed_to_draw = 0;

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

    public void IncreaseMaxEnergy(int amount) {
        max_energy += amount;

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
        player_unit.SetPosition(random_tile, new Vector3(0, 0.65f, 0));

        random_tile = tile_manager.GetRandomValid();
        player_unit.SetPath(random_tile);

        player_units.Add(player_unit);
    }

    void SpawnPlayerUnit(GameObject soldier_prefab, HexTile hex_tile) {
        GameObject new_player_unit = Instantiate(soldier_prefab);
        Unit player_unit = new_player_unit.GetComponent<Unit>();

        player_unit.SetPosition(hex_tile, new Vector3(0, 0.65f, 0));

        HexTile random_tile = tile_manager.GetRandomValid();
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
