using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField]
    int width = 11;

    [SerializeField]
    int height = 11;

    [SerializeField]
    GameObject hex_tile_pf;

    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        
    }

    void CreateGrid()
    {
        float tile_size = hex_tile_pf.transform.localScale.x * 1f;
        float tile_width = tile_size * 2;
        float tile_height = Mathf.Sqrt(3);

        Vector3 new_tile_location = new Vector3(-height / 2 * tile_height, 0, -width / 2 * tile_width);
        for(int r = 0; r < height; r++) { 
            new_tile_location += new Vector3(tile_height, 0, 0);
            new_tile_location.z = -width / 2 * tile_width;
            if(r % 2 == 0) { 
                new_tile_location.z += tile_width / 2;
            }
            for(int c = 0; c < width; c++) {
                new_tile_location += new Vector3(0, 0, tile_width);

                Instantiate(hex_tile_pf, new_tile_location, Quaternion.identity, transform);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
