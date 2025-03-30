using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileVisual : MonoBehaviour
{
    HexTile parent = null;

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.GetComponent<HexTile>();
    }

    public HexTile GetHexTile() {
        if(parent == null) {
            Debug.Log("Issue receiving parent");
        }

        return parent;
    }
}
