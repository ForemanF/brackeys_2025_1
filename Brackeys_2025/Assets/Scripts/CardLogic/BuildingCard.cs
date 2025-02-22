using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCard : CardAction
{
    [SerializeField]
    TileMesh building_mesh;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override public bool DisplayCardAction(HexTile hex_tile, Mesh temp_mesh) {
        hex_tile.SetTemporaryMesh(temp_mesh);

        return IsValidPlacement(hex_tile);
    }

    override public TileMesh GetTileMesh() {
        return building_mesh;
    }

    override public bool IsValidPlacement(HexTile hex_tile) {
        if(hex_tile.GetTileMesh() == TileMesh.Water) {
            return false;
        }

        return hex_tile.IsRevealed();
    }

    override public void PerformCardAction(HexTile hex_tile, HexMeshTuple hex_mesh_tuple) {
        hex_tile.SetTileMesh(hex_mesh_tuple);
    }
}
