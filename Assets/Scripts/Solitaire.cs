using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

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
    int showCards = 3;

    [SerializeField]
    float cardOffset_x = 0.4f;
    [SerializeField]
    float cardOffset_y = 0.2f;
    [SerializeField]
    float cardOffset_z = 0.02f;
    
    [SerializeField]
    float cardDealDelay = 0.01f;
    
    string [] cards = new string [] {
        "CA", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10", "CJ", "CQ", "CK",
        "DA", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D10", "DJ", "DQ", "DK",
        "HA", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H10", "HJ", "HQ", "HK",
        "SA", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "S10", "SJ", "SQ", "SK"
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
                }
            }
        }
    }

    public void CallShowCards() {
       StartCoroutine(ShowCards()) ;
    }

    bool isEmptyDeck = false;
    IEnumerator ShowCards () {
        // Remove any cards that are already showing 
        foreach(string s in show) {
            Destroy(GameObject.Find(s));
            discard.Add(s);
        }
        show.Clear();

        // Pull three cards from deck and show them
        for(int i=0;i<showCards;++i) {
            if(deck.Count != 0) {
                Vector3 position = new Vector3(deckButton.transform.position.x + 1.5f + cardOffset_x * i , 
                                               deckButton.transform.position.y,  
                                               deckButton.transform.position.z - cardOffset_z * i);
                GameObject newCard = Instantiate(cardPrefab, position, Quaternion.identity, deckButton.transform);
                newCard.name = deck[^1];
                // TODO: clean this up
                newCard.GetComponent<Selectable>().faceUp = true;
                show.Add(deck[^1]);
                deck.RemoveAt(deck.Count-1);
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
