using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UIElements;

public class Solitaire : MonoBehaviour
{
    [SerializeField]
    Sprite[] cardFaces;

    [SerializeField]
    GameObject cardPrefab;

    [SerializeField]
    GameObject[] topSlots;
    List<GameObject>[] topCards = new List<GameObject>[4].Select(item=>new List<GameObject>()).ToArray();

    [SerializeField]
    GameObject[] bottomSlots;
    List<GameObject>[] bottomCards = new List<GameObject>[7].Select(item=>new List<GameObject>()).ToArray();

    [SerializeField]
    GameObject deckButton;

    [SerializeField]
    int showNumCards = 3;

    [SerializeField]
    float cardOffset_x = 0.4f;
    [SerializeField]
    float cardOffset_y = 0.2f;
    [SerializeField]
    float cardOffset_z = 0.02f;
    
    [SerializeField]
    float cardDealDelay = 0.01f;
    
    string [] cards = new string [] {
        "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10", "C11", "C12", "C13",
        "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D10", "D11", "D12", "D13",
        "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H10", "H11", "H12", "H13",
        "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "S10", "S11", "S12", "S13"
    };

    List<GameObject> deck = new List<GameObject>(); 
    List<GameObject> show = new List<GameObject>();
    List<GameObject> discard = new List<GameObject>();

    Vector3 startPositionOffset = new Vector3(0,0,0.2f);
    Vector3 showPositionOffset = new Vector3(1.5f,0,0);

    Vector3 startPosition = new Vector3();
    Vector3 showPosition = new Vector3();

    public void PrintCards() {
        foreach(List<GameObject> l in bottomCards) {
            string s = "";
            foreach(GameObject g in l) {
                s += g.name + " ";
            }
            Debug.Log(s);
        }
    }

    void Start() {
        GenerateDeck();
        StartCoroutine(DealCards());
    }

    void Update() {
        
    }

    // Add cards to deck and shuffel
    void GenerateDeck() {
        startPosition = deckButton.transform.position + startPositionOffset;
        showPosition = deckButton.transform.position + showPositionOffset;
        foreach(string name in cards) {
            GameObject new_card = Instantiate(cardPrefab, startPosition, Quaternion.identity, deckButton.transform);
            new_card.name = name;
            deck.Add(new_card);
        }
        Shuffel(deck);
    }

    IEnumerator DealCards() {
        // Iterate over bottomCards in a triangle pattern
        for(int i=0; i<bottomCards.Count(); ++i) {
            for(int j=i; j<bottomCards.Count(); ++j) {

                // Get new position for card 
                Vector3 new_position = bottomSlots[j].transform.position + new Vector3(0, -cardOffset_y * i, -cardOffset_z * i);

                // Get next card from deck 
                GameObject next_card = deck.Last();
                deck.Remove(next_card);

                // Move card to new position and assign to array
                next_card.transform.position = new_position;
                bottomCards[j].Add(next_card);

                // If its the last card, make it face up  TODO: Clean this up
                if (i == j) { 
                    next_card.GetComponent<Selectable>().faceUp = true;
                    next_card.GetComponent<Selectable>().selectable = true;
                }
                yield return new WaitForSeconds(cardDealDelay);
            }
        }
    }

    public void CallShowCards() {
       StartCoroutine(ShowCards()) ;
    }

    public void CardClick(GameObject card) {
        HandleCardClick(card);
    }

    bool isEmptyDeck = false;
    IEnumerator ShowCards () {

        // Remove any cards that are already showing 
        foreach(GameObject s in show) {
            discard.Add(s);
            s.transform.position = startPosition;
            s.GetComponent<Selectable>().selectable = false;
            s.GetComponent<Selectable>().faceUp = false;
        }
        show.Clear();

        // Pull 'showNumCards' cards from deck and show them
        for(int i=0;i<showNumCards;++i) {
            if(deck.Count != 0) {
                // Grab next card
                Vector3 new_position = showPosition + new Vector3(cardOffset_x * i, 0, -cardOffset_z * i);
                GameObject next_card = deck.Last();

                // Remove it from deck and add to show 
                deck.Remove(next_card);
                show.Add(next_card);

                // Move to new position
                next_card.transform.position = new_position;

                // Turn face up
                next_card.GetComponent<Selectable>().faceUp = true;

                // If its the top card set it as selectable 
                if(deck.Count == 0 || i == showNumCards-1) {
                    next_card.GetComponent<Selectable>().selectable = true;
                }

                // Delay
                yield return new WaitForSeconds(cardDealDelay);
            }
        }

        // If deck is empty wait one round then pull the discard pile back 
        if(isEmptyDeck) {
            discard.Reverse();
            foreach(GameObject g in discard) {
                deck.Add(g);
            }
            discard.Clear();
            isEmptyDeck = false;
        }
        
        if(deck.Count == 0) {
            isEmptyDeck = true;
        }
    }

    // Get suit and number from card name
    (char, int) GetSuitAndNumber(string s) {
        return (s[0], int.Parse(s.Substring(1)));
    }

    int GetBottomSlotIndex(GameObject card) {
        // Return -1 if not in bottom slot and is not on top of deck
        for(int i=0;i<bottomCards.Count();++i) {
            for(int j=0; j<bottomCards[i].Count()-1; ++j){
                if(bottomCards[i][j] == card){ return i; }
            }
        }
        return -1;
    }

    void HandleCardClick(GameObject card) {

        // Only process clicks on selectable cards 
        if(card.GetComponent<Selectable>().selectable) {

            // Get bottom slot index (if card is not in bottom slot then result will be -1)
            int bottomSlotIndex = GetBottomSlotIndex(card);

            // Get suit and number
            (char card_suit, int card_number) = GetSuitAndNumber(card.name);

            // Handle Aces 
            if(card_number == 1) {
                // Check top slots
                for(int i=0;i<topCards.Count();++i) {
                    // If we have an empty slot    
                    if(topCards[i].Count == 0) {
                        // Remove card from prev list
                        PurgeCard(card);
                        // Add card to slot list
                        topCards[i].Add(card);
                        // Move card position to new one 
                        card.transform.position = topSlots[i].transform.position + new Vector3(0,0,-cardOffset_z);
                        return;
                    }
                }
            } 

            // Hande Remaining cards: check top slots
            // Only check top slots if card is not buried in bottom slots
            if(bottomSlotIndex == -1) {
                Debug.Log("Card has children! Slot index: " + bottomSlotIndex);
                for(int i=0;i<topCards.Count();++i) {
                    // 
                    if(topCards[i].Count != 0) {
                        (char suit, int number) = GetSuitAndNumber(topCards[i].Last().name);
                        // Check suit is same and card_number is one more 
                        if(card_suit == suit && card_number == number + 1){
                            PurgeCard(card);
                            topCards[i].Add(card);
                            card.transform.position = topSlots[i].transform.position + new Vector3(0,0,-cardOffset_z * topCards[i].Count);
                            //card.transform.SetParent(topSlots[i].transform);
                            return;
                        }
                    }
                }
            }

            // Handle Remaining cards: check bottom slots  
            for(int i=0;i<bottomCards.Count();++i) {
                if(bottomCards[i].Count !=0) {
                    (char suit, int number) = GetSuitAndNumber(bottomCards[i].Last().name);
                    // Check suit is oposite color and card_number is one less
                    if(     (((card_suit == 'C' || card_suit == 'S') && (suit == 'H' || suit == 'D'))
                            || ((card_suit == 'H' || card_suit == 'D') && (suit == 'C' || suit == 'S')))
                            && (card_number == number-1)) {

                        // log
                        Debug.Log("card_number: " + card_number + " number: " + number);
                        Debug.Log("card_suit: " + card_suit + " suit: " + suit);

                        //Vector3 new_position =  bottomSlots[i].transform.position + new Vector3(0, -cardOffset_y, -cardOffset_z) * bottomCards[i].Count;

                        // Handle case when card has children
                        if(bottomSlotIndex != -1) {
                            bottomSlotIndex = Math.Abs(bottomSlotIndex);

                            // Find the cards that need to be moved
                            bool found = false;
                            List<GameObject> move_cards = new List<GameObject>();
                            foreach(GameObject check_card in bottomCards[bottomSlotIndex]) {
                                if(found || check_card == card) { 
                                    // Move object
                                    move_cards.Add(check_card);
                                    found = true;
                                }
                            }

                            // Move the cards
                            foreach(GameObject move_card in move_cards) {
                                Vector3 new_position =  bottomSlots[i].transform.position + new Vector3(0, -cardOffset_y, -cardOffset_z) * bottomCards[i].Count;
                                PurgeCard(move_card);
                                bottomCards[i].Add(move_card);
                                move_card.transform.position = new_position;
                            }

                        } else {
                            Vector3 new_position =  bottomSlots[i].transform.position + new Vector3(0, -cardOffset_y, -cardOffset_z) * bottomCards[i].Count;
                            PurgeCard(card);
                            bottomCards[i].Add(card);
                            card.transform.position = new_position;
                        }

                        //card.transform.SetParent(bottomSlots[i].transform);
                        return;
                    }
                }
            }

            // Handle king posible move to blank space
            if (card_number == 13) {
                for(int i=0;i<bottomCards.Count();++i){
                    if(bottomCards[i].Count == 0) {
                        // Remove card from prev list
                        PurgeCard(card);
                        // Add card to slot list
                        bottomCards[i].Add(card);
                        // Move card position to new one 
                        card.transform.position = bottomSlots[i].transform.position + new Vector3(0,0,cardOffset_z);
                        return;
                    }
                }
                
            }

        }

        // Handle clicks on face down cards
        else {
            Debug.Log("Non selectable card: " + card.name);
            for(int i=0;i<bottomCards.Count();++i) {
                // If the card is on top then set it to be face up
                if(bottomCards[i].Count() != 0 && bottomCards[i].Last() == card) {
                    card.GetComponent<Selectable>().faceUp = true;
                    card.GetComponent<Selectable>().selectable = true;
                    return;
                }
            }
        }
    }

    bool PurgeCard(GameObject card) {
        // Clear from top cards
        foreach(List<GameObject> list in topCards) {
            if(list.Remove(card)) {
                return true;
            }
        }
        // Clear from bottom cards
        foreach(List<GameObject> list in bottomCards) {
            if(list.Remove(card)) {
                return true;
            }
        }
        // Clear from shown cards
        for(int i=0;i<show.Count;++i){
            if(card == show[i]){
                if(i>0) { show[i-1].GetComponent<Selectable>().selectable = true; }
                show.Remove(card);
                return true;
            }
        }
        // Return false if we did not find the card
        return false;
    }

    void Shuffel<T>(List<T> list) {
        System.Random random = new System.Random();
        for(int i=0; i<list.Count; ++i) { 
            int r = random.Next(0,list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    int GetCardIndex(string card_name) {
        for(int i=0; i<cards.Count(); ++i) {
            if(card_name == cards[i]) {
                return i;
            }
        }
        return -1;
    }

    public Sprite GetCardFace(string card_name) {
        int index = GetCardIndex(card_name);
        if(index < 0) { 
            Debug.Log("Error! " + card_name + "not recognized as card name");
            return null; 
        }
        return cardFaces[index];
    }
}
