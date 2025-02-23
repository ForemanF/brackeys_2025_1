using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCard : CardAction
{
    [SerializeField]
    TileMesh building_mesh;


    override public void DisplayCardAction(HexTile hex_tile, Mesh temp_mesh) {
        hex_tile.SetTemporaryMesh(temp_mesh);
    }

    override public TileMesh GetTileMesh() {
        return building_mesh;
    }

    override public void PerformCardAction(HexTile hex_tile, HexMeshTuple hex_mesh_tuple) {
        hex_tile.SetTileMesh(hex_mesh_tuple);

        EventBus.Publish(new CreateBuildingEvent(this, hex_tile));
    }
}
