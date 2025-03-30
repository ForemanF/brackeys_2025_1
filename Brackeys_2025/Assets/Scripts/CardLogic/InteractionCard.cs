using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionCard : CardAction
{
    [SerializeField]
    Faction affected_faction = Faction.Player;

    public void SetAffectedFaction(Faction faction) {
        affected_faction = faction;
    }

    override public bool IsValidPlacement(HexTile hex_tile, TileManager tile_manager, TileMesh building_mesh) {
        if(hex_tile == false) {
            return false;
        }
        GameObject game_obj = hex_tile.GetObjectOnHex();

        if(game_obj == null) {
            return false;
        }

        return game_obj.GetComponent<HasFaction>().GetFaction() == affected_faction;
    }

    override public void PerformCardAction(HexTile hex_tile, HexMeshTuple hex_mesh_tuple) {
        HasHealth has_health = hex_tile.GetObjectOnHex().GetComponent<HasHealth>();
        if(affected_faction == Faction.Enemy) {
            // do damage
            has_health.TakeDamage(strength);
        }
        else { 
            // heal
            has_health.Heal(health);

            EventBus.Publish(new ParticleBurstEvent(hex_tile.transform.position, BurstType.BuildHeal, 10));
        }

    }
}
