using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [SerializeField]
    int num_cards = 15;


    [SerializeField]
    GameObject card_parent;

    [SerializeField]
    GameObject card_pf;

    List<Card> all_cards;

    [SerializeField]
    List<Card> hand;

    [SerializeField]
    List<Card> draw_pile;

    [SerializeField]
    RectTransform hand_rt;

    [SerializeField]
    RectTransform draw_pile_rt;

    [SerializeField]
    float card_spacing = 100;

    [SerializeField]
    float draw_time_s = 1;

    Card current_highlighted_card = null;

    [SerializeField]
    float highlight_rise_amt_y = 100;

    Dictionary<Card, Coroutine> highlighters;

    // Start is called before the first frame update
    void Start()
    {
        all_cards = new List<Card>();
        CreateCards();

        hand = new List<Card>();
        draw_pile = new List<Card>();

        // Fill in the draw pile
        foreach(Card card in all_cards) {
            draw_pile.Add(card);
        }

        // Shuffle
        Shuffle();

        // Draw Cards to the hand
        //for(int i = 0; i < 3; ++i) {
        //    DrawCard();
        //}
        StartCoroutine(TestLoop());

        highlighters = new Dictionary<Card, Coroutine>();
    }

    IEnumerator TestLoop() { 
        while(true) {
            yield return new WaitForSeconds(1f);

            yield return HandleDrawCard();

            if(hand.Count > 6) {
                yield break;
            }
        }
    }

    void CreateCards() {
        int uid = 0;
        // Creates the actual card game objects
        for(int i = 0; i < num_cards; ++i) {
            GameObject new_card = Instantiate(card_pf, card_parent.transform);
            Card new_card_card = new_card.GetComponent<Card>();
            new_card_card.SetCardId(uid);

            new_card.name = "Card " + uid;

            new_card.transform.SetParent(draw_pile_rt, false);
            new_card.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

            all_cards.Add(new_card_card);

            uid++;
        }
    }

    void Shuffle() { 
        for(int i = draw_pile.Count - 2; i >= 1; i--) {
            int rand_int = Random.Range(0, i);

            // swap
            Card temp = draw_pile[i];
            draw_pile[i] = draw_pile[rand_int];
            draw_pile[rand_int] = temp;
        }
    }

    IEnumerator HandleDrawCard() {
        int starting_hand_size = hand.Count;
        List<Vector3> ending_positions = GetCardPositions(starting_hand_size + 1);

        Card drawn_card = DrawCard();
        int drawn_index = hand.Count - 1;

        // shift the cards already in the hand
        for(int i = 0; i < starting_hand_size; ++i) {
            hand[i].GoToPosition(ending_positions[i], draw_time_s);
        }

        // move the card from the deck to the hand
        drawn_card.GoToPosition(ending_positions[drawn_index], draw_time_s);

        for(int i = 0; i < hand.Count; ++i)
        {
            hand[i].SetBasePosition(ending_positions[i]);
        }

        yield return null;
    }

    public void HighlightCard(Card card) {
        if (card != current_highlighted_card) { 

            if(current_highlighted_card != null) { 
                current_highlighted_card.GoToBasePosition();
            }

            if(card == null) {
                current_highlighted_card = null;
                return;
            }

            Vector3 end_pos = card.GetBasePosition();
            end_pos.y += highlight_rise_amt_y;
            card.GoToPosition(end_pos);

            current_highlighted_card = card;
        }
    }

    Card DrawCard() {
        Card drawn_card = draw_pile[draw_pile.Count - 1];
        draw_pile.RemoveAt(draw_pile.Count - 1);
        hand.Add(drawn_card);

        drawn_card.transform.SetParent(hand_rt, true);

        return drawn_card;
    }

    List<Vector3> GetCardPositions(int hand_size) {
        if(hand_size == 0) {
            Debug.Log("Zero Hand size!");
            return new List<Vector3>();
        }

        List<Vector3> card_locations = new List<Vector3>();

        float x_spacing = 1.0f / Mathf.Sqrt(hand_size) * card_spacing;

        float start_x_pos = -x_spacing * ((hand_size - 1) / 2.0f);

        float current_x = start_x_pos;

        for(int i = 0; i < hand_size; ++i) {
            card_locations.Add(new Vector3(current_x, 0, 0));

            current_x += x_spacing;
        }

        return card_locations;
    }

}
