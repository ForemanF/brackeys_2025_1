using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barracks : Building
{
    [SerializeField]
    GameObject soldier_prefab;

    int turns_passed = 0;
    public override IEnumerator ProcessBuildingAction()
    {
        turns_passed++;

        if (turns_passed / special_value >= 1)
        {
            HexTile spawn_hex = null;
            foreach (HexTile adj_tile in current_tile.GetNeighbors())
            {
                if (adj_tile.GetTileState() == TileState.Empty)
                {
                    spawn_hex = adj_tile;
                }
            }

            if (spawn_hex != null)
            {
                EventBus.Publish(new SpawnSoldierEvent(spawn_hex, soldier_prefab));
                turns_passed = 0;
            }
            else { 
                Debug.Log("No Available palce to spawn unit");
            }

        }

        yield return null;
    }
}
