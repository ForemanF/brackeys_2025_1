using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCard : CardAction
{
    [SerializeField]
    GameObject soldier_prefab;

    override public void PerformCardAction(HexTile hex_tile, HexMeshTuple hex_mesh_tuple) {
        EventBus.Publish(new SpawnSoldierEvent(hex_tile, soldier_prefab));
    }
}
