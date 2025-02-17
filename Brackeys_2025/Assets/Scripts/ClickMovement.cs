using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickMovement : MonoBehaviour
{
    [SerializeField]
    TileManager tm;

    HexTile current_tile;
    List<HexTile> path = null;

    [SerializeField]
    Vector3 offset = Vector3.zero;

    bool reached_goal = true;
    HexTile destination = null;

    LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        current_tile = tm.GetCenterTile();
        transform.position = current_tile.transform.position + offset;

        tm.RevealTile(current_tile);

        lr = GetComponent<LineRenderer>();

        StartCoroutine(MovementLoop());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator MovementLoop()
    {
        while (true)
        {
            yield return HandleMovement();
        }
    }

    public void SetPath(HexTile destination) { 
        if(reached_goal == false) {
            return;
        }
        path = PathFinder.FindPath(current_tile, destination);
        UpdateLineRenderer(path);
        Debug.Log("setting new goal");
    }

    IEnumerator HandleMovement()
    {
        if (path != null && path.Count != 0)
        {
            HexTile next_tile = path[path.Count - 1];

            Vector3 next_pos = next_tile.transform.position + offset;

            GameObject new_obj = new GameObject("LookAtObj");
            Transform look_at = new_obj.transform;
            look_at.position = next_tile.transform.position;
            look_at.position += offset;

            transform.LookAt(look_at);

            UpdateLineRenderer(path);
            yield return LerpUtilities.LerpToPosition(transform, next_pos, 1);

            current_tile = next_tile;
            tm.RevealTile(current_tile);
            path.RemoveAt(path.Count - 1);

            if(path.Count == 0) {
                reached_goal = true;
            }
        }
        yield return null;
    }

    void UpdateLineRenderer(List<HexTile> tiles) { 
        if(lr == null) {
            return;
        }

        List<Vector3> points = new List<Vector3>();
        foreach (HexTile tile in tiles) {
            points.Add(tile.transform.position + offset);
        }

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());

    }
}
