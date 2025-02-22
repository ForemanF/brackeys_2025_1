using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAction : MonoBehaviour
{
    public enum CardType { 
        Undefined,
        Building, 
        Unit,
        Interaction
    }

    [SerializeField]
    CardType card_type = CardType.Undefined;

    [SerializeField]
    int energy_cost = 1;

    virtual public TileMesh GetTileMesh() {
        return TileMesh.Grass;
    }

    public CardType GetCardType() {
        return card_type; 
    }

    public int GetCost() {
        return energy_cost; 
    }

    virtual public bool DisplayCardAction(HexTile hex_tile, Mesh temp_mesh) {
        return IsValidPlacement(hex_tile);
    }

    virtual public void PerformCardAction(HexTile hex_tile, HexMeshTuple hex_mesh_tuple) {
        Debug.Log("Parent Call, Nothing Happens");
    }

    virtual public bool IsValidPlacement(HexTile hex_tile) {
        return hex_tile.IsRevealed();
    }
}
