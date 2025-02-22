using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable] public class IntToCardAction { public int id; public CardAction card_action; }

public class DeckManager : MonoBehaviour
{
    [SerializeField]
    int num_cards = 15;

    [SerializeField]
    IntToCardAction[] int_to_card_action_list;

    Dictionary<int, CardAction> card_action_dict;

    [SerializeField]
    GameObject card_parent;

    [SerializeField]
    int max_hand_size = 5;

    [SerializeField]
    GameObject card_pf;

    List<Card> all_cards;

    List<Card> hand;

    List<Card> draw_pile;

    List<Card> discard_pile;

    [SerializeField]
    RectTransform hand_rt;

    [SerializeField]
    RectTransform draw_pile_rt;

    [SerializeField]
    RectTransform discard_pile_rt;

    [SerializeField]
    float card_spacing = 100;

    [SerializeField]
    float draw_time_s = 1;

    Card current_highlighted_card = null;

    [SerializeField]
    float highlight_rise_amt_y = 100;

    Subscription<DiscardCardEvent> discard_card_sub;

    [SerializeField]
    TextMeshProUGUI hand_count_text;


    int hand_card_layer;
    int non_hand_card_layer;

    // Start is called before the first frame update
    void Start()
    {
        hand_card_layer = LayerMask.NameToLayer("HandCard");
        non_hand_card_layer = LayerMask.NameToLayer("NonHandCard");


        card_action_dict = new Dictionary<int, CardAction>();
        foreach(IntToCardAction int_to_action in int_to_card_action_list) {
            card_action_dict[int_to_action.id] = int_to_action.card_action;
        }

        all_cards = new List<Card>();
        CreateCards();

        hand = new List<Card>();
        draw_pile = new List<Card>();
        discard_pile = new List<Card>();

        UpdateHandCountText();

        // Fill in the draw pile
        foreach(Card card in all_cards) {
            draw_pile.Add(card);
        }

        // Shuffle
        Shuffle();

        discard_card_sub = EventBus.Subscribe<DiscardCardEvent>(_OnDiscardCard);
    }

    void UpdateHandCountText() {
        hand_count_text.text = hand.Count.ToString() + "/" + max_hand_size.ToString();
    }

    void _OnDiscardCard(DiscardCardEvent e) {
        HandleDiscardCard(e.discarded_card);
    }

    bool IsHandFull() { 
        if(hand.Count >= max_hand_size) {
            return true;
        }
        return false;
    }

    public IEnumerator DrawCards(int num_cards) {
        for(int i = 0; i < num_cards; ++i) { 
            // check to see if hand is full
            if(IsHandFull()) {
                Debug.Log("FullHand");
                yield break;
            }

            HandleDrawCard();

            yield return new WaitForSeconds(draw_time_s);
        }
        
    }

    void CreateCards() {
        int uid = 0;
        // Creates the actual card game objects
        for(int i = 0; i < num_cards; ++i) {
            GameObject new_card = Instantiate(card_pf, card_parent.transform);
            Card new_card_card = new_card.GetComponent<Card>();
            new_card_card.SetCardId(uid);

            new_card_card.SetCardAction(card_action_dict[1]); // always set to 1 for now

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

    void HandleDrawCard() {
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
            hand[i].SetBoundingBoxLayer(hand_card_layer);
        }

        UpdateHandCountText();
    }

    void HandleDiscardCard(Card discard_card) {
        List<Vector3> ending_positions = GetCardPositions(hand.Count - 1);

        DiscardCard(discard_card);

        // shift the cards already in the hand
        for(int i = 0; i < hand.Count; ++i) {
            hand[i].GoToPosition(ending_positions[i], draw_time_s);
        }

        // move the card from the deck to the hand
        discard_card.GoToBasePosition();

        for(int i = 0; i < hand.Count; ++i)
        {
            hand[i].SetBasePosition(ending_positions[i]);
        }

        discard_card.SetBoundingBoxLayer(non_hand_card_layer);

        UpdateHandCountText();
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

    void DiscardCard(Card discarded_card, bool destroy_card = false) {
        // find the discarded card in the hand
        int index = -1;
        for(int i = 0; i < hand.Count; ++i) {
            if (hand[i] == discarded_card) {
                index = i;
            }
        }

        if(index == -1) {
            Debug.LogError("Trying to discard a card that isn't in your hand");
            return;
        }

        hand.RemoveAt(index);
        discard_pile.Add(discarded_card);
        discarded_card.transform.SetParent(discard_pile_rt, true);
        discarded_card.SetBasePosition(discard_pile_rt.anchoredPosition);
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
