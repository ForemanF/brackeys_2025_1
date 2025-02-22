using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PathFinder
{
    public static List<HexTile> FindPath(HexTile origin, HexTile destination) {
        Dictionary<HexTile, Node> nodes_not_evaluated = new Dictionary<HexTile, Node>();
        Dictionary<HexTile, Node> nodes_already_evaluated = new Dictionary<HexTile, Node>();

        Node start_node = new Node(origin, origin, destination, 0);
        nodes_not_evaluated.Add(origin, start_node);

        bool got_path = EvaluateNextNode(nodes_not_evaluated, nodes_already_evaluated, origin, destination, out List<HexTile> path);

        while(got_path == false) { 
            got_path = EvaluateNextNode(nodes_not_evaluated, nodes_already_evaluated, origin, destination, out path);
        }

        return path;
    }

    private static bool EvaluateNextNode(Dictionary<HexTile, Node> nodes_not_evaluated,
        Dictionary<HexTile, Node> nodes_evaluated, HexTile origin, HexTile destination,
        out List<HexTile> path)
    {
        Node[] nodes_not_evaluated_values = new Node[nodes_not_evaluated.Count];
        nodes_not_evaluated.Values.CopyTo(nodes_not_evaluated_values, 0);
        Node current_node = GetCheapestNode(nodes_not_evaluated_values);

        if(current_node == null) {
            path = new List<HexTile>();
            return false;
        }

        nodes_not_evaluated.Remove(current_node.target);
        nodes_evaluated.Add(current_node.target, current_node);

        path = new List<HexTile>();

        if(current_node.target == destination) {
            path.Add(current_node.target);

            while(current_node.target != origin) {
                path.Add(current_node.parent.target);
                current_node = current_node.parent;
            }

            return true;
        }

        List<Node> neighbors = new List<Node>();

        foreach (HexTile hex_tile in current_node.target.GetNeighbors()) {
            Node node = new Node(hex_tile, origin, destination, current_node.GetCost());

            if(hex_tile.GetTileState() != TileState.Default) {
                node.base_cost = Mathf.Infinity;
            }

            neighbors.Add(node);
        }

        foreach(Node neighbor in neighbors) { 
            if(nodes_evaluated.ContainsKey(neighbor.target)) {
                continue;
            }

            if(neighbor.GetCost() < current_node.GetCost() || !nodes_not_evaluated.ContainsKey(neighbor.target)) {
                neighbor.SetParent(current_node);
                if(!nodes_not_evaluated.ContainsKey(neighbor.target)) {
                    nodes_not_evaluated.Add(neighbor.target, neighbor);
                }
            }
        }

        return false;
    }

    private static Node GetCheapestNode(Node[] nodes_not_evaluated) { 
        if(nodes_not_evaluated.Length == 0) {
            return null;
        }

        Node selected_node = nodes_not_evaluated[0];

        for(int i = 0; i < nodes_not_evaluated.Length; ++i) {
            var current_node = nodes_not_evaluated[i];

            if(current_node.GetCost() < selected_node.GetCost()) {
                selected_node = current_node;
            }
            else if(current_node.GetCost() == selected_node.GetCost() && current_node.cost_to_destination < selected_node.cost_to_destination) {
                selected_node = current_node;
            }
        }

        return selected_node;
    }
}
