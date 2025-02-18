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
    float draw_speed_s = 1;

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
            yield return new WaitForSeconds(5f);

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
        List<Vector3> starting_positions = GetCardPositions(starting_hand_size);
        List<Vector3> ending_positions = GetCardPositions(starting_hand_size + 1);

        float start_time = Time.time;
        float progress = 0;

        Card drawn_card = DrawCard();
        int drawn_index = hand.Count - 1;
        Vector3 drawn_start_pos = drawn_card.GetComponent<RectTransform>().anchoredPosition;


        while(progress < 1) {
            progress = (Time.time - start_time) / draw_speed_s;

            // shift the cards already in the hand
            for(int i = 0; i < starting_hand_size; ++i) {
                hand[i].GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(starting_positions[i], ending_positions[i], progress);
            }

            // move the card from the deck to the hand
            drawn_card.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(drawn_start_pos, ending_positions[drawn_index], progress);

            yield return null;
        }

        for(int i = 0; i < hand.Count; ++i) {
            hand[i].GetComponent<RectTransform>().anchoredPosition = ending_positions[i];
        }
        drawn_card.GetComponent<RectTransform>().anchoredPosition = ending_positions[drawn_index];

        foreach(Card card in hand) {
            card.SetBasePosition(card.GetComponent<RectTransform>().anchoredPosition);
        }

        yield return null;
    }

    public void HighlightCard(Card card) {
        if (card != current_highlighted_card) { 

            if(current_highlighted_card != null) { 
                if(highlighters.TryGetValue(current_highlighted_card, out Coroutine rise_co)) {
                    StopCoroutine(rise_co);
                    highlighters[current_highlighted_card] = StartCoroutine(LowerCard(current_highlighted_card));
                }
            }

            if(card == null) {
                current_highlighted_card = null;
                return;
            }

            if (highlighters.TryGetValue(card, out Coroutine lower_co))
            {
                StopCoroutine(lower_co);
            }

            highlighters[card] = StartCoroutine(RiseCard(card));

            current_highlighted_card = card;
        }
    }

    IEnumerator RiseCard(Card card) {
        float start_time = Time.time;
        float progress = 0;

        Vector3 start_pos = card.GetComponent<RectTransform>().anchoredPosition;

        Vector3 goal_pos = card.GetBasePosition();
        goal_pos.y += highlight_rise_amt_y;

        while(progress < 1) {
            progress = (Time.time - start_time) / 0.4f;
            card.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(start_pos, goal_pos, progress);

            yield return null;
        }

        card.GetComponent<RectTransform>().anchoredPosition = goal_pos;
    }

    IEnumerator LowerCard(Card card) {
        float start_time = Time.time;
        float progress = 0;

        Vector3 start_pos = card.GetComponent<RectTransform>().anchoredPosition;

        Vector3 goal_pos = card.GetBasePosition();

        while(progress < 1) {
            progress = (Time.time - start_time) / 1;
            card.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(start_pos, goal_pos, progress);

            yield return null;
        }

        card.GetComponent<RectTransform>().anchoredPosition = goal_pos;
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

    // Update is called once per frame
    void Update()
    {
    }
}
