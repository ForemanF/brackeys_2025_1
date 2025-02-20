using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


}
