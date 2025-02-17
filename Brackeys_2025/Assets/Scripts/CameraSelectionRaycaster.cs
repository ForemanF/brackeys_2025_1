using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSelectionRaycaster : MonoBehaviour
{
    [SerializeField]
    Camera my_camera;

    [SerializeField]
    ClickMovement click_movement;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = my_camera.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit)) {
            Transform object_hit = hit.transform;

            HexTileVisual target;
            if(object_hit.TryGetComponent(out target)) {
                HexTile highlighted_tile = target.GetHexTile();

                EventBus.Publish(new HighlightTileEvent(highlighted_tile));

                if(Input.GetMouseButtonDown(0)) {
                    click_movement.SetPath(highlighted_tile);
                }
            }
        }
    }
}
