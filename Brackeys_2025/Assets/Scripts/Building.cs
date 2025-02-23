using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{

    protected HexTile current_tile;

    public void SetHex(HexTile hex_tile) {
        current_tile = hex_tile;
        current_tile.OccupyHex(gameObject, TileState.Building);
    }

    public HexTile GetHex() {
        return current_tile;
    }

    virtual public IEnumerator ProcessBuildingAction() {
        yield return null;
    
    }

    virtual public IEnumerator ProcessBuildingAttack(HexTile nearest_enemy) {
        yield return null;
    }

}
