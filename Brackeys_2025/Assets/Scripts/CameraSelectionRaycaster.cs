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

    // card that is actively clicked and moving
    [SerializeField]
    Card selected_card = null;

    int ui_layer;

    // Start is called before the first frame update
    void Start()
    {
        ui_layer = LayerMask.NameToLayer("UI");
    }

    // Update is called once per frame
    void Update()
    {
        Card current_card = HighlightCard();

        MoveCard(current_card);

        if(current_card == null && selected_card == null) { 
            SceneMouseInteraction();
        }
    }

    void SceneMouseInteraction() { 
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

    void MoveCard(Card card) { 
        // If left click, start setting the position of the card
        if(Input.GetMouseButtonDown(0) && card != null) {
            // check if it is a UI item
            card.StopCurrentCoroutine();

            if (selected_card != null) {
                selected_card.GoToBasePosition();
            }

            // return the previous selected card back to its home
            selected_card = card;
        }
        else if(Input.GetMouseButtonUp(0)) {
            // if released, check if it fulfills a valid action, if not, return 
            // the card to your hand
            if(selected_card != null) { 
                selected_card.GoToBasePosition();
            }

            selected_card = null;
        }
        else if(selected_card != null) {
            // set the position of the card
            Vector3 mouse_position = ScreenToRectPos(selected_card.GetComponent<RectTransform>().parent.GetComponent<RectTransform>(), Input.mousePosition);
            selected_card.GetComponent<RectTransform>().anchoredPosition = mouse_position;

            // TODO: Highlight valid game actions
        }
    }

    public Vector2 ScreenToRectPos(RectTransform rect_transform, Vector2 screen_pos)
    { 
        Vector2 anchorPos= screen_pos - new Vector2(rect_transform.position.x, rect_transform.position.y);
        anchorPos= new Vector2(anchorPos.x / rect_transform.lossyScale.x, anchorPos.y / rect_transform.lossyScale.y);
        return anchorPos;
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

    Card HighlightCard() { 
        List<RaycastResult> hits = GetEventSystemRaycastResults();

        for (int index = 0; index < hits.Count; index++)
        {
            RaycastResult curRaysastResult = hits[index];
            if (curRaysastResult.gameObject.layer == ui_layer) {
                GameObject hit_ui_elem = curRaysastResult.gameObject;
                Card highlight_card = hit_ui_elem.GetComponentInParent<Card>();

                if(highlight_card != selected_card && selected_card == null) { 
                    deck_manager.HighlightCard(highlight_card);
                }

                return highlight_card;
            }
        }
        if(selected_card == null) {
            deck_manager.HighlightCard(null);
        }
        return null;
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
