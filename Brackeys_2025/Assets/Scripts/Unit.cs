using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Unit : MonoBehaviour
{
    [SerializeField]
    int strength = 1;

    HexTile current_tile;
    List<HexTile> path = null;

    Vector3 offset = Vector3.zero;

    GameObject look_at_obj;

    HasFaction has_faction;

    [SerializeField]
    float movement_speed = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        look_at_obj = new GameObject("LookAtObj");

        has_faction = GetComponent<HasFaction>();
    }

    public IEnumerator ProcessTurn() {
        HideLogic(current_tile);
        GameObject opposing_obj = CanAttack();

        if (opposing_obj != null)
        {
            yield return Attack(opposing_obj);
        }
        else { 
            yield return HandleMovement();
        }

        yield return null;
    }

    public HexTile GetHex() {
        return current_tile;
    }

    IEnumerator Attack(GameObject obj_to_attack) {
        // Look in the direction of attack
        LookAtPosition(obj_to_attack.transform.position);

        HasHealth obj_to_attack_health = obj_to_attack.GetComponent<HasHealth>();

        Vector3 starting_pos = transform.position;

        // Need to add a vertical component and an animation curve
        yield return LerpUtilities.LerpToPosition(transform, obj_to_attack.transform.position, 0.3f);

        obj_to_attack_health.TakeDamage(strength);

        yield return LerpUtilities.LerpToPosition(transform, starting_pos, 0.3f);
    }

    GameObject CanAttack() {
        // check if an adjacent tile has an opposing team's unit on it
        List<HexTile> neighbors = current_tile.GetNeighbors();
        foreach(HexTile adjacent_tile in neighbors) {
            GameObject obj_on_hex = adjacent_tile.GetObjectOnHex();
            if(obj_on_hex == null) {
                continue;
            }

            if(obj_on_hex.TryGetComponent(out HasFaction adj_faction)) { 
                if(has_faction.GetFaction() != adj_faction.GetFaction()) {
                    return obj_on_hex;
                } 
            }
        }

        return null;
    }

    void LookAtPosition(Vector3 target_position, bool maintain_steady_pitch = true) { 
        Transform look_at = look_at_obj.transform;
        Vector3 look_position = target_position;

        if(maintain_steady_pitch) {
            look_position.y = transform.position.y;
        
        }

        transform.LookAt(look_position);
    }

    public void SetPosition(HexTile hex_tile, Vector3 _offset) {
        offset = _offset;
        current_tile = hex_tile;
        transform.position = current_tile.transform.position + offset;

        current_tile.OccupyHex(gameObject, TileState.HasUnit);

        StartCoroutine(ProcessOnNextFrame(current_tile));
    }

    public bool HasTarget() { 
        if (path == null || path.Count == 0) {
            return false;
        }
        return true;
    }

    public void SetPath(HexTile destination) { 
        path = PathFinder.FindPath(current_tile, destination);

        if (path[path.Count - 1] == current_tile) {
            // Last tile is my tile
            path.RemoveAt(path.Count - 1);
        }
    }

    public IEnumerator HandleMovement()
    {
        if (path != null && path.Count != 0)
        {
            // leaving this hex
            current_tile.EmptyHex();

            HexTile next_tile = path[path.Count - 1];

            if(current_tile == next_tile) {
                Debug.LogError("Moving to the same hex, not good!");
            }

            Vector3 next_pos = next_tile.transform.position + offset;
            LookAtPosition(look_at_obj.transform.position);

            Transform look_at = look_at_obj.transform;
            look_at.position = next_tile.transform.position;
            look_at.position += offset;

            transform.LookAt(look_at);

            HideLogic(next_tile);

            yield return LerpUtilities.LerpToPosition(transform, next_pos, movement_speed);
            next_tile.OccupyHex(gameObject, TileState.HasUnit);

            current_tile = next_tile;
            path.RemoveAt(path.Count - 1);
        }
        else {
            Debug.Log("Nowhere to go");
        }
        yield return null;
    }

    IEnumerator ProcessOnNextFrame(HexTile next_tile) {
        yield return null;
        HideLogic(next_tile);
    }

    void HideLogic(HexTile next_tile) {
        if(GetComponent<HasFaction>().GetFaction() == Faction.Enemy) { 
            if (next_tile.IsRevealed())
            {
                Utilities.SetLayerAndChildren(transform, "Default");
            }
            else
            {
                Utilities.SetLayerAndChildren(transform, "Hidden");
            }
        }
        else {
            EventBus.Publish(new RevealTileEvent(next_tile));
        }

    }
}
