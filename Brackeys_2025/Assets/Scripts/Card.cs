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
}
