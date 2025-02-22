using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Unit : MonoBehaviour
{
    public enum Faction { 
        Player,
        Enemy
    }

    HexTile current_tile;
    List<HexTile> path = null;

    Vector3 offset = Vector3.zero;

    [SerializeField]
    Faction my_faction = Faction.Enemy;

    GameObject look_at_obj;

    // Start is called before the first frame update
    void Start()
    {
        look_at_obj = new GameObject("LookAtObj");
    }

    public IEnumerator ProcessTurn() {
        bool can_attack = CanAttack();

        if (can_attack == true)
        {
            yield return Attack();
        }
        else { 
            yield return HandleMovement();
        }

        yield return null;
    }

    IEnumerator Attack() {
        yield return null;
    }

    bool CanAttack() {
        // attacks an adjacent tile, returns false if it can't find anything

        return false;
    }

    public void SetPosition(HexTile hex_tile, Vector3 _offset) {
        offset = _offset;
        current_tile = hex_tile;
        transform.position = current_tile.transform.position + offset;

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
            Debug.Log("Moving");
            HexTile next_tile = path[path.Count - 1];

            Vector3 next_pos = next_tile.transform.position + offset;

            Transform look_at = look_at_obj.transform;
            look_at.position = next_tile.transform.position;
            look_at.position += offset;

            transform.LookAt(look_at);

            HideLogic(next_tile);

            yield return LerpUtilities.LerpToPosition(transform, next_pos, 1);

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
        Debug.Log(my_faction);

        if(my_faction == Faction.Enemy) { 
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
