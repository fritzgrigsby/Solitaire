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
    [SerializeField] Sprite[] cardFaces;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject deckButton;

    [Header("Card Slots")]
    [SerializeField] GameObject[] foundation;
    [SerializeField] GameObject[] tableau;
    [SerializeField] GameObject[] hand;
    enum HandSlots {STOCK, FAN, WASTE};
    CardList fanList;
    CardList wasteList;
    //CardList stockList, fanList, wasteList;

    [Header("Card Settings")]
    [SerializeField] int showNumCards = 3;
    [SerializeField] float cardOffset_x = -0.4f;
    [SerializeField] float cardOffset_y = 0.2f;
    [SerializeField] float cardDealDelay = 0.01f;
    
    string [] cards = new string [] {
        "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10", "C11", "C12", "C13",
        "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D10", "D11", "D12", "D13",
        "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H10", "H11", "H12", "H13",
        "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "S10", "S11", "S12", "S13"
    };

    RecordList recordList = new RecordList();

    void Start() {
        fanList = hand[(int)HandSlots.FAN].GetComponent<CardList>();
        wasteList = hand[(int)HandSlots.WASTE].GetComponent<CardList>();
        InitCardSlots();
        GenerateDeck();
        StartCoroutine(DealCards());
    }

    void InitCardSlots() {
        // Set hand slot offsets
        hand[(int)HandSlots.FAN].GetComponent<CardList>().SetPlacementOffsets(cardOffset_x, 0);

        // Set tableau slot offsets
        foreach(var t in tableau) {
            t.GetComponent<CardList>().SetPlacementOffsets(0, cardOffset_y);
        }
    }

    // Add cards to deck and shuffel
    void GenerateDeck() {
        var stock = hand[(int)HandSlots.STOCK].GetComponent<CardList>();
        foreach(string name in cards) {
            GameObject new_card = Instantiate(cardPrefab, stock.transform.position, Quaternion.identity);
            new_card.name = name;
            stock.Push(new_card);
        }
        stock.Shuffel();
    }

    IEnumerator DealCards() {
        // Get card list from stock
        var stock = hand[(int)HandSlots.STOCK].GetComponent<CardList>();

        // Iterate over tableau in a triangle pattern
        for(int i=0; i<tableau.Count(); ++i) {
            for(int j=i; j<tableau.Count(); ++j) {

                // Set delay
                yield return new WaitForSeconds(cardDealDelay);

                // Get next card from stock
                GameObject next_card = stock.Pop();

                // If its the last card, make it face up and selectable
                if (i == j) { 
                    next_card.GetComponent<Selectable>().faceUp = true;
                    next_card.GetComponent<Selectable>().selectable = true;
                }

                // Push next card to tabeleau
                tableau[j].GetComponent<CardList>().Push(next_card);
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

        // Get stock and fan
        var stock = hand[(int)HandSlots.STOCK].GetComponent<CardList>();
        var fan = hand[(int)HandSlots.FAN].GetComponent<CardList>();
        var waste = hand[(int)HandSlots.WASTE].GetComponent<CardList>();

        // Remove any cards that are already showing 
        recordList.Push(fan.Front(), fan, waste);
        while(!fan.IsEmpty()) {
            var card = fan.PopFront();
            card.GetComponent<Selectable>().selectable = false;
            card.GetComponent<Selectable>().faceUp = false;
            card.SetActive(false);
            waste.Push(card);
        }

        // Pull 'showNumCards' cards from deck and add to fan 
        if(!stock.IsEmpty()) { recordList.Push(stock.Top(), stock, fan); }
        for(int i=0;i<showNumCards;++i) {

            if(!stock.IsEmpty()) {

                var card = stock.Pop();
                card.GetComponent<Selectable>().faceUp = true;

                // If its the top card set it as selectable 
                if(stock.IsEmpty() || i == showNumCards-1) {
                    card.GetComponent<Selectable>().selectable = true;
                }

                fan.Push(card);
                yield return new WaitForSeconds(cardDealDelay);
            }
        }

        // If deck is empty wait one round then pull the discard pile back 
        if(isEmptyDeck) {

            recordList.Push(waste.Top(), waste, stock);
            while(!waste.IsEmpty()) {
                var card = waste.Pop();
                stock.Push(card);
                card.SetActive(true);
            }
            isEmptyDeck = false;
        }
        
        if(stock.IsEmpty()) {
            isEmptyDeck = true;
        }
    }

    // Get suit and number from card name
    (char, int) GetSuitAndNumber(string s) {
        return (s[0], int.Parse(s.Substring(1)));
    }

    GameObject CheckTopOfTabelaeu(string card_name) {
        foreach(var t in tableau) {
            var list = t.GetComponent<CardList>();
            if(!list.IsEmpty() && list.Top().name == card_name && list.Top().GetComponent<Selectable>().selectable) {
                return t;
            }
        }
        return null;
    }

    GameObject CheckTopOfFoundation(string card_name) {
        foreach(var f in foundation) {
            var list = f.GetComponent<CardList>();
            if(!list.IsEmpty() && list.Top().name == card_name && list.Top().GetComponent<Selectable>().selectable) {
                return f;
            }
        }
        return null;
    }

    void Undo() {
        var reverse = recordList.Pop();
        // If from and to slots are the same, fip and make card unselectable 
        if(reverse.fromSlot == reverse.toSlot) {
            Debug.Log("Flip Card");
            reverse.card.GetComponent<Selectable>().selectable = false;
            reverse.card.GetComponent<Selectable>().faceUp = false;
            return;
        }

        // Get list of cards to reverse
        var pop_list = reverse.toSlot.PopFrom(reverse.card);

        // If fromSlot is fanList, we need to reverse it, flip and make all cards unselectable 
        if(reverse.toSlot == fanList) { 
            Debug.Log("Is fan list");
            pop_list.Reverse();
            foreach(var p in pop_list) {
                p.GetComponent<Selectable>().selectable = false;
                p.GetComponent<Selectable>().faceUp = false;
                reverse.fromSlot.Push(p);
            }
            // Run Undo again to pull back cards from waste
            if(!wasteList.IsEmpty()) {
                Undo();
            }
        }
        // If from waste list we need to make cards active again
        else if(reverse.toSlot == wasteList) {
            foreach(var p in pop_list) {
                p.SetActive(true);
                p.GetComponent<Selectable>().faceUp = true;
                reverse.fromSlot.Push(p);
            }
        }
        // Default: Move cards back 
        else {
            foreach(var p in pop_list) {
                reverse.fromSlot.Push(p);
            }
        }
    }

    // I know: card clicked, Card slot of card
    // I need to know: (destination) if card can move to foundation, if card can move to tabelaeu
    //                  If card is not on the top 
    void HandleCardClick(GameObject card) {

        if(card.name == "Undo") {
            Debug.Log("Undo");
            Undo();
        }

        if(!fanList.IsEmpty()) {
            fanList.Top().GetComponent<Selectable>().selectable = true;
        }

        // Only move selectable cards
        if(card.GetComponent<Selectable>().selectable) {
            (char card_suit, int card_number) = GetSuitAndNumber(card.name);
            GameObject  move_to_slot;

            // Special Case Ace
            if(card_number == 1) {
                foreach(var f in foundation) {
                    var list = f.GetComponent<CardList>();
                    if(list.IsEmpty()) {
                        var card_parent_list = card.transform.parent.GetComponent<CardList>();
                        card_parent_list.Pop();
                        list.Push(card);
                        recordList.Push(card, card_parent_list, list);
                        return;
                    }
                }
            }

            // Check Foundation
            string find_card_foundation_name = card_suit + (card_number-1).ToString(); 
            move_to_slot = CheckTopOfFoundation(find_card_foundation_name);
            if(move_to_slot != null) {
                var card_parent_list =  card.transform.parent.GetComponent<CardList>();
                var pop_list = card_parent_list.PopFrom(card);
                var move_to_list = move_to_slot.GetComponent<CardList>();
                foreach(var p in pop_list) {
                    move_to_list.Push(p);
                }
                recordList.Push(card, card_parent_list, move_to_list);
                return;
            }

            // Check Tabelaeu
            char tabeleau_suit1, tabeleau_suit2;
            if(card_suit == 'H' || card_suit == 'D') {
                tabeleau_suit1 = 'C'; tabeleau_suit2 = 'S';
            } else {
                tabeleau_suit1 = 'H'; tabeleau_suit2 = 'D';
            }
            string find_card_tabeleau_name1 = tabeleau_suit1 + (card_number+1).ToString(); 
            string find_card_tabeleau_name2 = tabeleau_suit2 + (card_number+1).ToString(); 

            move_to_slot = CheckTopOfTabelaeu(find_card_tabeleau_name1);
            if(move_to_slot == null) {
                move_to_slot = CheckTopOfTabelaeu(find_card_tabeleau_name2);
            }
            if(move_to_slot != null) {
                var card_parent_list = card.transform.parent.GetComponent<CardList>();
                var pop_list = card_parent_list.PopFrom(card);
                var move_to_list = move_to_slot.GetComponent<CardList>();
                foreach(var p in pop_list) {
                    move_to_list.Push(p);
                }
                recordList.Push(card, card_parent_list, move_to_list);
                return;
            }


            // Special Case King
            if(card_number == 13) {
                foreach(var t in tableau) {
                    var list = t.GetComponent<CardList>();
                    if(list.IsEmpty())  {
                        var card_parent_list = card.transform.parent.GetComponent<CardList>();
                        var pop_list = card.transform.parent.GetComponent<CardList>().PopFrom(card);
                        foreach(var p in pop_list) {
                            list.Push(p);
                        }
                        recordList.Push(card, card_parent_list, list);
                        return;

                    }
                }

            }

        } 
        // If card is not selectable and top of tabelaeu then fip it and make it selectable 
        else {
            foreach(var t in tableau) {
                var list = t.GetComponent<CardList>();
                if(!list.IsEmpty() && list.Top().name == card.name) {
                    card.GetComponent<Selectable>().selectable = true;
                    card.GetComponent<Selectable>().faceUp = true;
                    recordList.Push(card, list, list);
                }
            }
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
