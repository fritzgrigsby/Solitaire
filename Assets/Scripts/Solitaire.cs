using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    List<string> [] topCards = new List<string> [4];

    [SerializeField]
    GameObject[] bottomSlots;
    List<string> [] bottomCards = new List<string> [7];

    string [] cards = new string [] {
        "CA", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10", "CJ", "CQ", "CK",
        "DA", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D10", "DJ", "DQ", "DK",
        "HA", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H10", "HJ", "HQ", "HK",
        "SA", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "S10", "SJ", "SQ", "SK"
    };

    List<string> deck = new List<string>(); 

    void Start() {
        GenerateDeck();
        DealCards();
    }

    void Update() {
        
    }

    void InitCards() {
    }

    // Add cards to deck and shuffel
    void GenerateDeck() {
        foreach(string card in cards) {
            deck.Add(card);
        }
        Shuffel(deck);
    }

    void DealCards() {
        float y_offset = -.2f;
        float z_offset = -.02f;
        Vector3 position = new Vector3(transform.position.x, transform.position.y,transform.position.z);
        foreach(string card in deck) {
            position.y += y_offset;
            position.z += z_offset;
            GameObject newCard = Instantiate(cardPrefab, position, Quaternion.identity);
            newCard.name = card;
        }

        for(int i=0; i<bottomSlots.Count(); ++i) {
            for(int j=i; j<bottomSlots.Count(); ++j) {
                GameObject newCard = Instantiate(cardPrefab, bottomSlots[j].transform.position , Quaternion.identity, bottomSlots[j].transform);
                newCard.name = deck[^1];
                //bottomCards[j].Add(deck[^1]);
                deck.RemoveAt(deck.Count-1);
            }
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
