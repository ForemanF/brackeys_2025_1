using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    [SerializeField]
    TileManager tm;

    HexTile current_tile;
    List<HexTile> path = null;

    // Start is called before the first frame update
    void Start()
    {
        current_tile = tm.GetRandomValid();
        transform.position = current_tile.transform.position + new Vector3(0, 1, 0);

        StartCoroutine(MovementLoop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator MovementLoop() { 
        while(true) {
            yield return HandleMovement();
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator HandleMovement() { 
        if(path == null || path.Count == 0) {
            HexTile destination = tm.GetRandomValid();
            path = PathFinder.FindPath(current_tile, destination);
        }
        else { 
            if(path.Count != 0) {
                HexTile next_tile = path[path.Count - 1];

                Vector3 next_pos = next_tile.transform.position + new Vector3(0, 1, 0);

                GameObject new_obj = new GameObject("LookAtObj");
                Transform look_at = new_obj.transform;
                look_at.position = next_tile.transform.position;
                look_at.position += new Vector3(0, 1, 0);

                transform.LookAt(look_at);

                if(next_tile.IsRevealed()) {
                    Utilities.SetLayerAndChildren(transform, "Default");
                }
                else { 
                    Utilities.SetLayerAndChildren(transform, "Hidden");
                }

                yield return LerpUtilities.LerpToPosition(transform, next_pos, 1);

                current_tile = next_tile;
                path.RemoveAt(path.Count - 1);
            }
        }
        yield return null;
    }
}
