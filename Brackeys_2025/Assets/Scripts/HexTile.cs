using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum TileType { 
    Default,
    Water
}

public class HexTile : MonoBehaviour
{
    [SerializeField]
    TileType tile_type = TileType.Default;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
