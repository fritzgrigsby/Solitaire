using System;
using System.Collections;
using System.Collections.Generic;
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
    List<string>[] topCards = new List<string>[4].Select(item=>new List<string>()).ToArray();

    [SerializeField]
    GameObject[] bottomSlots;
    List<string>[] bottomCards = new List<string>[7].Select(item=>new List<string>()).ToArray();

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

    public List<string> deck = new List<string>(); 
    public List<string> show = new List<string>();
    public List<string> discard = new List<string>();

    void Start() {
        GenerateDeck();
        StartCoroutine(DealCards());
    }

    void Update() {
        
    }

    // Add cards to deck and shuffel
    void GenerateDeck() {
        foreach(string card in cards) {
            deck.Add(card);
        }
        Shuffel(deck);
    }

    IEnumerator DealCards() {
        for(int i=0; i<bottomSlots.Count(); ++i) {
            for(int j=i; j<bottomSlots.Count(); ++j) {
                yield return new WaitForSeconds(cardDealDelay);
                Vector3 position = new Vector3(bottomSlots[j].transform.position.x, 
                                 bottomSlots[j].transform.position.y - cardOffset_y * i, 
                                 bottomSlots[j].transform.position.z - cardOffset_z * i);
                GameObject newCard = Instantiate(cardPrefab, position, Quaternion.identity, bottomSlots[j].transform);
                newCard.name = deck[^1];
                if(bottomCards[j] == null) {
                    bottomCards[j] = new List<string>();
                }
                bottomCards[j].Add(deck[^1]);
                deck.RemoveAt(deck.Count-1);

                // If its the last card, make it face up  TODO: Clean this up
                if (i == j) { 
                    newCard.GetComponent<Selectable>().faceUp = true;
                    newCard.GetComponent<Selectable>().selectable = true;
                }
            }
        }
    }

    public void CallShowCards() {
       StartCoroutine(ShowCards()) ;
    }

    public void CardClick(GameObject card) {
        // If card 
        HandleCardClick(card);
    }

    bool isEmptyDeck = false;
    IEnumerator ShowCards () {
        // Remove any cards that are already showing 
        foreach(string s in show) {
            Destroy(GameObject.Find(s));
            discard.Add(s);
        }
        show.Clear();

        // Pull 'showNumCards' cards from deck and show them
        for(int i=0;i<showNumCards;++i) {
            if(deck.Count != 0) {
                Vector3 position = new Vector3(deckButton.transform.position.x + 1.5f + cardOffset_x * i , 
                                               deckButton.transform.position.y,  
                                               deckButton.transform.position.z - cardOffset_z * i);
                GameObject newCard = Instantiate(cardPrefab, position, Quaternion.identity, deckButton.transform);
                newCard.name = deck[^1];
                newCard.GetComponent<Selectable>().faceUp = true;
                show.Add(deck[^1]);
                deck.RemoveAt(deck.Count-1);

                // If its the top card set it as selectable 
                if(deck.Count == 0 || i == showNumCards-1) {
                    newCard.GetComponent<Selectable>().selectable = true;
                }

                // Delay
                yield return new WaitForSeconds(cardDealDelay);
            }
        }

        // If deck is empty wait one round then pull the discard pile back 
        if(isEmptyDeck) {
            discard.Reverse();
            Debug.Log("Deck is empty");
            foreach(string s in discard) {
                deck.Add(s);
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

    void HandleCardClick(GameObject card) {

        // Only process clicks on selectable cards 
        if(card.GetComponent<Selectable>().selectable) {

            // Get suit and number
            (char card_suit, int card_number) = GetSuitAndNumber(card.name);

            // Handle Aces 
            if(card_number == 1) {
                // Check top slots
                for(int i=0;i<topCards.Count();++i) {
                    if(topCards[i].Count == 0) {
                        PurgeCard(card.name);
                        topCards[i].Add(card.name);
                        card.transform.position = topSlots[i].transform.position;
                        card.transform.SetParent(topSlots[i].transform);
                        return;
                    }
                }
            } 
            // Handle Kings
            /*
            else if(card_number == 13) {

            }
            */
            // Handle All other cards
            else {
                // Hande top slots first
                for(int i=0;i<topCards.Count();++i) {
                    if(topCards[i].Count != 0) {
                        (char suit, int number) = GetSuitAndNumber(topCards[i][^1]);
                        // Check suit is same and card_number is one more 
                        if(card_suit == suit && card_number == number + 1){
                            PurgeCard(card.name);
                            topCards[i].Add(card.name);
                            card.transform.position = topSlots[i].transform.position;
                            card.transform.SetParent(topSlots[i].transform);
                            return;
                        }
                    }
                }
                // Handle bottom slots
                for(int i=0;i<bottomCards.Count();++i) {
                    if(bottomCards[i].Count !=0) {
                        (char suit, int number) = GetSuitAndNumber(bottomCards[i][^1]);
                        // Check suit is oposite color and card_number is one less
                        if(     ((card_suit == 'C' || card_suit == 'S') && (suit == 'H' || suit == 'D'))
                             || ((card_suit == 'H' || card_suit == 'D') && (suit == 'C' || suit == 'S'))) {
                            if(card_number == number-1) {
                                Debug.Log("card_number: " + card_number + " number: " + number);
                                Debug.Log("card_suit: " + card_suit + " suit: " + suit);
                                Debug.Log("BottomCard count: " + bottomCards.Count());
                                Vector3 position = new Vector3(bottomSlots[i].transform.position.x,
                                                            bottomSlots[i].transform.position.y - cardOffset_y * bottomCards[i].Count(),
                                                            bottomSlots[i].transform.position.z - cardOffset_z * bottomCards[i].Count());
                                PurgeCard(card.name);
                                bottomCards[i].Add(card.name);
                                card.transform.position = position;
                                card.transform.SetParent(bottomSlots[i].transform);
                                return;
                            }
                        }
                    }
                }
                Debug.Log("Selectable other card: " + card.name);
            }

        }

        // Handle clicks on face down cards
        else {
            Debug.Log("Non selectable card: " + card.name);
            for(int i=0;i<bottomCards.Count();++i) {
                // If the card is on top then set it to be face up
                if(bottomCards[i][^1] == card.name) {
                    card.GetComponent<Selectable>().faceUp = true;
                    card.GetComponent<Selectable>().selectable = true;
                }
            }
        }
    }

    bool PurgeCard(string card_name) {
        // Clear from top cards
        foreach(List<string> t in topCards) {
            foreach(string s in t) {
                if(s == card_name) {
                    t.Remove(s);
                    return true;
                }
            }
        }
        // Clear from bottom cards
        foreach(List<string> b in bottomCards) {
            foreach(string s in b) {
                if(s == card_name) {
                    b.Remove(s);
                    return true;
                }
            }
        }
        // Clear from shown cards
        if(show[^1] == card_name) {
            show.Remove(card_name);
            return true;
        }
        // Return false if we did not find the card
        Debug.Log("Failure puring card: " + card_name);
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
