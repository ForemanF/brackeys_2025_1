using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{

    public Node parent;
    public HexTile target;
    public HexTile destination;
    public HexTile origin;

    public float base_cost;
    public float cost_from_origin;
    public float cost_to_destination;
    public float path_cost;

    public Node(HexTile current, HexTile origin, HexTile destination, float path_cost) {
        parent = null;
        this.target = current;
        this.origin = origin;
        this.destination = destination;

        base_cost = 1;
        cost_from_origin = Vector3Int.Distance(current.GetCubeCoords(), origin.GetCubeCoords());
        cost_to_destination = Vector3Int.Distance(current.GetCubeCoords(), destination.GetCubeCoords());

        this.path_cost = path_cost;
    }

    public float GetCost() {
        return path_cost + base_cost + cost_from_origin + cost_to_destination;
    }

    public void SetParent(Node node) {
        this.parent = node;
    }

}
