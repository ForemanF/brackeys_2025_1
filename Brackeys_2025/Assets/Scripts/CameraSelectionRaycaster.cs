using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CameraSelectionRaycaster : MonoBehaviour
{
    [SerializeField]
    Camera my_camera;

    [SerializeField]
    DeckManager deck_manager;

    [SerializeField]
    GameManager game_manager;

    // card that is actively clicked and moving
    [SerializeField]
    Card selected_card = null;

    [SerializeField]
    RectTransform health_display = null;

    [SerializeField]
    TextMeshProUGUI health_text = null;

    [SerializeField]
    float ui_back_height = 280;

    [SerializeField]
    float time_before_transparent_s = 3;

    [SerializeField]
    AnimationCurve transparent_curve = null;

    float time_current_card_selected = 0;

    int hand_card_layer;
    int ui_back_layer;
    int ui_layer;

    // Start is called before the first frame update
    void Start()
    {
        hand_card_layer = LayerMask.NameToLayer("HandCard");
        ui_back_layer = LayerMask.NameToLayer("UIBack");
        ui_back_layer = LayerMask.NameToLayer("UI");
    }

    // Update is called once per frame
    void Update()
    {
        Card current_card = HighlightCard();

        MoveCard(current_card);

        HighlightMouseTile();
    }

    HexTile GetMouseTile() { 
        RaycastHit hit;
        Ray ray = my_camera.ScreenPointToRay(Input.mousePosition);

        HexTile hex_tile = null;
        if (Physics.Raycast(ray, out hit))
        {
            Transform object_hit = hit.transform;

            HexTileVisual target;
            if (object_hit.TryGetComponent(out target))
            {
                hex_tile = target.GetHexTile();
            }
        }
        return hex_tile;
    }

    void HighlightMouseTile() {
        HexTile highlighted_tile = GetMouseTile();

        if(highlighted_tile != null) { 
            EventBus.Publish(new HighlightTileEvent(highlighted_tile));
        }

        // get health of the tile
        if(highlighted_tile != null) {
            GameObject obj_on_hex = highlighted_tile.GetObjectOnHex();

            if(obj_on_hex != null && highlighted_tile.IsRevealed() && obj_on_hex.TryGetComponent<HasHealth>(out HasHealth has_health)) {
                health_display.gameObject.SetActive(true);
                int health = has_health.GetHealth();
                health_text.text = health.ToString();

                Transform tf = highlighted_tile.transform.GetChild(1);
                Vector3 hex_position = Camera.main.WorldToScreenPoint(tf.position);
                Vector3 mouse_position = ScreenToRectPos(health_display.parent.GetComponent<RectTransform>(), hex_position);
                health_display.anchoredPosition = mouse_position;
            }
            else { 
                health_display.gameObject.SetActive(false);
            }
        }
    }

    void MoveCard(Card card) {
        HexTile cur_hex_tile = GetMouseTile();

        // If left click, start setting the position of the card
        if(Input.GetMouseButtonDown(0) && card != null) {
            // check if it is a UI item
            card.StopCurrentCoroutine();

            // return the previous selected card back to its home
            if (selected_card != null) {
                selected_card.GoToBasePosition();
            }

            // check if we can pay for the card
            int amount = card.GetCardAction().GetCost();
            bool has_enough_energy = game_manager.HasEnoughEnergy(amount);

            if(has_enough_energy == false) {
                selected_card = null;
            }
            else { 
                selected_card = card;
                time_current_card_selected = 0;
            }

        }
        else if(Input.GetMouseButtonUp(0)) {
            // if released, check if it fulfills a valid action, if not, return 
            // the card to your hand

            if(selected_card != null) { 
                EventBus.Publish(new TakeCardActionEvent(cur_hex_tile, selected_card));
                //selected_card.GoToBasePosition();

                selected_card.SetAlpha(1);
            }

            selected_card = null;
        }
        else if(selected_card != null) {
            // set the position of the card
            Vector3 mouse_position = ScreenToRectPos(selected_card.GetComponent<RectTransform>().parent.GetComponent<RectTransform>(), Input.mousePosition);
            selected_card.GetComponent<RectTransform>().anchoredPosition = mouse_position;

            if(mouse_position.y < 280) { 
                // card is behind the border
                time_current_card_selected -= Time.deltaTime * 2;
            }
            else { 
                time_current_card_selected += Time.deltaTime;

                if(cur_hex_tile != null) {
                    EventBus.Publish(new HighlightCardActionEvent(cur_hex_tile, selected_card));
                }
            }

            time_current_card_selected = Mathf.Clamp(time_current_card_selected, 0, time_before_transparent_s);

            float time_percent_passed = Mathf.Clamp01(time_current_card_selected / time_before_transparent_s);

            float alpha = transparent_curve.Evaluate(time_percent_passed);

            selected_card.SetAlpha(alpha);
        }
    }

    public Vector2 ScreenToRectPos(RectTransform rect_transform, Vector2 screen_pos)
    { 
        Vector2 anchorPos= screen_pos - new Vector2(rect_transform.position.x, rect_transform.position.y);
        anchorPos= new Vector2(anchorPos.x / rect_transform.lossyScale.x, anchorPos.y / rect_transform.lossyScale.y);
        return anchorPos;
    }
    
    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverLayerElement(int layer)
    {
        return IsPointerOverLayerElement(GetEventSystemRaycastResults(), layer);
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverLayerElement(List<RaycastResult> eventSystemRaysastResults, int layer)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == layer)
                return true;
        }
        return false;
    }

    Card HighlightCard() { 
        List<RaycastResult> hits = GetEventSystemRaycastResults();

        for (int index = 0; index < hits.Count; index++)
        {
            RaycastResult curRaysastResult = hits[index];
            if (curRaysastResult.gameObject.layer == hand_card_layer) {
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
