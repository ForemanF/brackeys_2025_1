using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using UnityEngine.UI;

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

    [SerializeField]
    string description = "";

    [SerializeField]
    Texture2D icon;

    [SerializeField]
    Sprite special_svg_image;

    [SerializeField]
    protected int health = 3;

    [SerializeField]
    protected int strength = 1;

    [SerializeField]
    protected int special_value = 0;

    public string GetDescription() {
        return description;
    }

    public int GetHealth() {
        return health;
    }

    public Sprite GetSpecialSVG() {
        return special_svg_image;
    }

    public int GetStrength() {
        return strength;
    }

    public int GetSpecialValue() {
        return special_value;
    }

    public Texture2D GetTexture() {
        return icon;
    }

    virtual public TileMesh GetTileMesh() {
        return TileMesh.Grass;
    }

    public CardType GetCardType() {
        return card_type; 
    }

    public int GetCost() {
        return energy_cost; 
    }

    virtual public void DisplayCardAction(HexTile hex_tile, Mesh temp_mesh) {
        Debug.Log("Parent Call, Nothing Happens");
    }

    virtual public void PerformCardAction(HexTile hex_tile, HexMeshTuple hex_mesh_tuple) {
        Debug.Log("Parent Call, Nothing Happens");
    }

    virtual public bool IsValidPlacement(HexTile hex_tile, TileManager tile_manager, TileMesh building_mesh) {
        bool added_rules = true;
        if(building_mesh != null) {
            added_rules = tile_manager.DoesSatisfyAllRestrictions(building_mesh, hex_tile);
        }

        return added_rules && hex_tile != null && hex_tile.IsRevealed() && hex_tile.GetTileState() == TileState.Empty;
    }
}
