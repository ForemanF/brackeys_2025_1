using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    Subscription<NextTurnEvent> next_turn_event;

    bool is_ready_for_next_turn = true;

    [SerializeField]
    TileManager tile_manager;

    [SerializeField]
    GameObject enemy_prefab;

    [SerializeField]
    GameObject player_unit_prefab;

    List<Unit> enemies;
    List<Unit> player_units;

    // Start is called before the first frame update
    void Start()
    {
        enemies = new List<Unit>();
        player_units = new List<Unit>();

        next_turn_event = EventBus.Subscribe<NextTurnEvent>(_OnNextTurn);

        cost_text_color = cost_text.color;
        UpdateCostVisual();

        tile_manager.RevealTile(tile_manager.GetCenterTile());

        SpawnEnemy();
        SpawnPlayerUnit();
    }

    public bool IsReadyForNextTurn() {
        return is_ready_for_next_turn;
    }

    void _OnNextTurn(NextTurnEvent e) {
        turn_number++;

        StartCoroutine(ProcessTurn());
    }

    IEnumerator ProcessUnitActions(List<Unit> units) {
        foreach(Unit unit in units) {
            if(unit.HasTarget() == false) {
                unit.SetPath(tile_manager.GetRandomValid());
            }

            yield return unit.ProcessTurn();
        }

        yield return null;
    }

    IEnumerator ProcessTurn() {
        is_ready_for_next_turn = false;

        // my buildings attack

        // my units move/attack
        yield return ProcessUnitActions(player_units);

        // enemy units move/attack
        yield return ProcessUnitActions(enemies);

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

}
