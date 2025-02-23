using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{

    protected HexTile current_tile;

    [SerializeField]
    protected int strength = -1;

    [SerializeField]
    protected int special_value = -1;

    public void SetHex(HexTile hex_tile) {
        current_tile = hex_tile;
        current_tile.OccupyHex(gameObject, TileState.Building);
    }

    public void SetSpecialValue(int _special_val) {
        special_value = _special_val;
    }

    public int GetSpecialValue() {
        return special_value;
    }

    public void SetStrength(int _strength) {
        strength = _strength;
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
