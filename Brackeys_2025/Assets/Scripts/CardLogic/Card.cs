using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VectorGraphics;

public class Card : MonoBehaviour
{
    public enum CardState { 
        Default,
        DrawPile,
        Hand,
        Discard
    }

    CardState card_state = CardState.Default;

    int card_id = -1;

    Vector3 base_position = Vector3.zero;

    Coroutine movement_coroutine = null;

    [SerializeField]
    float lerp_speed = 100;

    [SerializeField]
    CardAction card_action;

    // Visual Components
    [SerializeField]
    Image[] images;

    [SerializeField]
    RawImage[] raw_images;

    [SerializeField]
    SVGImage[] svg_images;

    [SerializeField]
    TextMeshProUGUI[] texts;

    [SerializeField]
    Image card_bounding_box;

    [SerializeField]
    TextMeshProUGUI cost_text;

    [SerializeField]
    TextMeshProUGUI heart_text;

    [SerializeField]
    GameObject heart_icon;

    [SerializeField]
    TextMeshProUGUI damage_text;

    [SerializeField]
    GameObject damage_icon;

    [SerializeField]
    GameObject special_icon;

    [SerializeField]
    SVGImage special_svg;

    [SerializeField]
    TextMeshProUGUI special_value_text;

    [SerializeField]
    RawImage card_image;

    [SerializeField]
    TextMeshProUGUI card_title_text;

    [SerializeField]
    TextMeshProUGUI card_desc_text;


    public void SetBoundingBoxLayer(int layer) {
        card_bounding_box.gameObject.layer = layer;
    }

    public void SetCardAction(CardAction _card_action) {
        card_action = _card_action;

        cost_text.text = card_action.GetCost().ToString();

        heart_text.text = card_action.GetHealth().ToString();
        if(card_action.GetHealth() >= 1) {
            heart_icon.SetActive(true);
        }
        else { 
            heart_icon.SetActive(false);
        }

        if(card_action.GetSpecialSVG() != null) {
            special_svg.sprite = card_action.GetSpecialSVG();
            special_value_text.text = card_action.GetSpecialValue().ToString();
            special_icon.SetActive(true);
        }
        else { 
            special_icon.SetActive(false);
        }

        damage_text.text = card_action.GetStrength().ToString();
        if(card_action.GetStrength() >= 1) {
            damage_icon.SetActive(true);
        }
        else { 
            damage_icon.SetActive(false);
        }

        card_image.texture = card_action.GetTexture();

        card_title_text.text = card_action.name;
        card_desc_text.text = card_action.GetDescription();
    }

    public CardAction GetCardAction() {
        return card_action;
    }

    void SetCardState(CardState new_card_state) {
        card_state = new_card_state;
    }

    public void SetCardId(int new_card_id) {
        card_id = new_card_id;
    }

    public void SetBasePosition(Vector3 new_base_position) {
        base_position = new_base_position;
    }

    public Vector3 GetBasePosition() {
        return base_position;
    }

    public void GoToBasePosition(float lerp_time = -1) { 
        Vector3 start_pos = GetComponent<RectTransform>().anchoredPosition;

        Vector3 end_pos = GetBasePosition();

        StopCurrentCoroutine();
        movement_coroutine = StartCoroutine(MoveCard(start_pos, end_pos, lerp_time));
    
    }

    public void GoToPosition(Vector3 end_pos, float lerp_time = -1) { 
        Vector3 start_pos = GetComponent<RectTransform>().anchoredPosition;

        StopCurrentCoroutine();
        movement_coroutine = StartCoroutine(MoveCard(start_pos, end_pos, lerp_time));
    }

    public void StopCurrentCoroutine() { 
        if(movement_coroutine != null) {
            StopCoroutine(movement_coroutine);
        }
    }

    IEnumerator MoveCard(Vector3 start_pos, Vector3 end_pos, float lerp_time) { 
        float start_time = Time.time;
        float progress = 0;

        float total_time = lerp_time;
        if(total_time <= 0) { 
            total_time = Vector3.Distance(start_pos, end_pos) / lerp_speed;
        }

        while(progress < 1) {
            progress = (Time.time - start_time) / total_time;
            GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(start_pos, end_pos, progress);

            yield return null;
        }

        GetComponent<RectTransform>().anchoredPosition = end_pos;
    }

    public void SetAlpha(float alpha) {
        foreach(Image image in images) {
            Color cur_color = image.color;
            cur_color.a = alpha;
            image.color = cur_color;
        }

        foreach (RawImage image in raw_images)
        {
            Color cur_color = image.color;
            cur_color.a = alpha;
            image.color = cur_color;
        }

        foreach (SVGImage image in svg_images)
        {
            Color cur_color = image.color;
            cur_color.a = alpha;
            image.color = cur_color;
        }

        foreach (TextMeshProUGUI text in texts)
        {
            Color cur_color = text.color;
            cur_color.a = alpha;
            text.color = cur_color;
        }


    }
}
