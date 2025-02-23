using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCard : CardAction
{
    [SerializeField]
    TileMesh building_mesh;

    [SerializeField]
    int health = 3;

    [SerializeField]
    int strength = 1;

    [SerializeField]
    int range = 0;

    public int GetHealth() {
        return health;
    }

    public int GetStrength() {
        return strength;
    }

    public int GetRange() {
        return range;
    }

    override public bool DisplayCardAction(HexTile hex_tile, Mesh temp_mesh) {
        hex_tile.SetTemporaryMesh(temp_mesh);

        return IsValidPlacement(hex_tile);
    }

    override public TileMesh GetTileMesh() {
        return building_mesh;
    }

    //override public bool IsValidPlacement(HexTile hex_tile) {
    //    return hex_tile.IsRevealed();
    //}

    override public void PerformCardAction(HexTile hex_tile, HexMeshTuple hex_mesh_tuple) {
        hex_tile.SetTileMesh(hex_mesh_tuple);

        EventBus.Publish(new CreateBuildingEvent(this, hex_tile));
    }
}
