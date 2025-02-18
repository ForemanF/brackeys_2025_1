using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraSelectionRaycaster : MonoBehaviour
{
    [SerializeField]
    Camera my_camera;

    [SerializeField]
    ClickMovement click_movement;

    [SerializeField]
    DeckManager deck_manager;

    int ui_layer;

    // Start is called before the first frame update
    void Start()
    {
        ui_layer = LayerMask.NameToLayer("UI");
    }

    // Update is called once per frame
    void Update()
    {
        HighlightCard();
        //print(IsPointerOverUIElement() ? "Over UI" : "Not over UI");

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

    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == ui_layer)
                return true;
        }
        return false;
    }

    void HighlightCard() { 
        List<RaycastResult> hits = GetEventSystemRaycastResults();

        Card selected_card = null;
        for (int index = 0; index < hits.Count; index++)
        {
            RaycastResult curRaysastResult = hits[index];
            if (curRaysastResult.gameObject.layer == ui_layer) {
                GameObject hit_ui_elem = curRaysastResult.gameObject;
                selected_card = hit_ui_elem.GetComponentInParent<Card>();
                deck_manager.HighlightCard(selected_card);
                return;
            }
        }
        deck_manager.HighlightCard(null);
    }

    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults;
    }

}
