using System.Collections;
using System.Linq;
using UnityEngine;

public class Solitaire : MonoBehaviour
{
    [Header("Card Data")]
    [SerializeField] Sprite[] cardFaces;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject deckButton;

    [Header("Card Slots")]
    [SerializeField] GameObject[] foundation;
    [SerializeField] GameObject[] tableau;
    [SerializeField] GameObject[] hand;
    CardList[] foundationLists;
    CardList[] tableauLists;
    CardList stockList, fanList, wasteList;

    [Header("Card Settings")]
    [SerializeField] int showNumCards = 3;
    [SerializeField] float cardOffset_x = -0.4f;
    [SerializeField] float cardOffset_y = 0.2f;
    [SerializeField] float cardDealDelay = 0.01f;
    
    string [] cardNames = new string [] {
        "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10", "C11", "C12", "C13",
        "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D10", "D11", "D12", "D13",
        "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H10", "H11", "H12", "H13",
        "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "S10", "S11", "S12", "S13"
    };

    // Record List for tracking moves to enable undo 
    public RecordList recordList = new RecordList();

    void Start() {
        InitCardSlots();
        GenerateDeck();
        StartCoroutine(DealCards());
    }

    void InitCardSlots() {
        // Cash hand card lists
        stockList = hand[0].GetComponent<CardList>();
        fanList = hand[1].GetComponent<CardList>();
        wasteList = hand[2].GetComponent<CardList>();

        // Cash foundation card lists
        foundationLists = new CardList[foundation.Count()];
        for(int i=0; i<foundation.Count(); ++i) {
            foundationLists[i] = foundation[i].GetComponent<CardList>();
        }

        // Cash tableau card lists
        tableauLists = new CardList[tableau.Count()];
        for(int i=0; i<tableau.Count(); ++i) {
            tableauLists[i] = tableau[i].GetComponent<CardList>();
        }

        // Set hand slot offsets
        fanList.SetPlacementOffsets(cardOffset_x, 0);

        // Set tableau slot offsets
        foreach(var t in tableau) {
            t.GetComponent<CardList>().SetPlacementOffsets(0, cardOffset_y);
        }
    }

    // Add cards to deck and shuffel
    void GenerateDeck() {
        for(int i=0; i<cardNames.Count(); ++i) {
            GameObject new_card = Instantiate(cardPrefab, stockList.transform.position, Quaternion.identity);
            new_card.name = cardNames[i];
            new_card.GetComponent<UpdateSprite>().SetCardFront(cardFaces[i]);
            stockList.Push(new_card);
        }
        stockList.Shuffel();
    }

    IEnumerator DealCards() {

        // Iterate over tableau in a triangle pattern
        for(int i=0; i<tableau.Count(); ++i) {
            for(int j=i; j<tableau.Count(); ++j) {

                // Set delay
                yield return new WaitForSeconds(cardDealDelay);

                // Get next card from stock
                GameObject next_card = stockList.Pop();

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

        // Remove any cards that are already showing 
        if(!fanList.IsEmpty()) { recordList.Push(fanList.Front(), fanList, wasteList); }
        while(!fanList.IsEmpty()) {
            var card = fanList.PopFront();
            card.GetComponent<Selectable>().selectable = false;
            card.GetComponent<Selectable>().faceUp = false;
            card.SetActive(false);
            wasteList.Push(card);
        }

        // Pull 'showNumCards' cards from deck and add to fan 
        if(!stockList.IsEmpty()) { recordList.Push(stockList.Top(), stockList, fanList); }
        for(int i=0;i<showNumCards;++i) {

            if(!stockList.IsEmpty()) {

                var card = stockList.Pop();
                card.GetComponent<Selectable>().faceUp = true;

                // If its the top card set it as selectable 
                if(stockList.IsEmpty() || i == showNumCards-1) {
                    card.GetComponent<Selectable>().selectable = true;
                }

                fanList.Push(card);
                yield return new WaitForSeconds(cardDealDelay);
            }
        }

        // If deck is empty wait one round then pull the discard pile back 
        if(isEmptyDeck) {

            recordList.Push(wasteList.Top(), wasteList, stockList);
            while(!wasteList.IsEmpty()) {
                var card = wasteList.Pop();
                stockList.Push(card);
                card.SetActive(true);
            }
            isEmptyDeck = false;
        }
        
        if(stockList.IsEmpty()) {
            isEmptyDeck = true;
        }
    }

    // Get suit and number from card name
    (char, int) GetSuitAndNumber(string s) {
        return (s[0], int.Parse(s.Substring(1)));
    }

    CardList CheckTopOfTabelaeu(string card_name) {
        foreach(var t in tableauLists) {
            if(!t.IsEmpty() && t.Top().name == card_name && t.Top().GetComponent<Selectable>().selectable) {
                return t;
            }
        }
        return null;
    }

    CardList CheckTopOfFoundation(string card_name) {
        foreach(var f in foundationLists) {
            if(!f.IsEmpty() && f.Top().name == card_name && f.Top().GetComponent<Selectable>().selectable) {
                return f;
            }
        }
        return null;
    }

    public void UndoAll() {
        if(SettingsMenu.menuOpen) { return; }
        while(!recordList.IsEmpty()) {
            Undo();
        }
    }

    public void Undo() {
        if(SettingsMenu.menuOpen) { return; }
        if(recordList.IsEmpty()) {
            //TODO: Add warning message?
            return;
        }
        var reverse = recordList.Pop();

        // If from and to slots are the same, fip and make card unselectable 
        if(reverse.fromSlot == reverse.toSlot) {
            reverse.card.GetComponent<Selectable>().selectable = false;
            reverse.card.GetComponent<Selectable>().faceUp = false;
            return;
        }

        // Get list of cards to reverse
        var pop_list = reverse.toSlot.PopFrom(reverse.card);

        // If fromSlot is fanList, we need to reverse it, flip and make all cards unselectable 
        if(reverse.toSlot == fanList) { 
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
        // If from stock list we need to make cards inactive and flip the waste list
        else if(reverse.toSlot == stockList) {
            while(pop_list.Count() != 0) {
                var p = pop_list.Last();
                p.SetActive(false);
                reverse.fromSlot.Push(p);
                pop_list.RemoveAt(pop_list.Count - 1);
            }
        }
        // Default: Move cards back 
        else {
            foreach(var p in pop_list) {
                reverse.fromSlot.Push(p);
            }
        }
    }

    void HandleCardClick(GameObject card) {

        // Make sure the top of fan is selectable 
        if(!fanList.IsEmpty()) {
            fanList.Top().GetComponent<Selectable>().selectable = true;
        }

        // Only move selectable cards
        if(card.GetComponent<Selectable>().selectable) {
            (char card_suit, int card_number) = GetSuitAndNumber(card.name);
            CardList move_to_list;

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
            move_to_list = CheckTopOfFoundation(find_card_foundation_name);
            if(move_to_list != null) {
                var card_parent_list =  card.transform.parent.GetComponent<CardList>();
                if(card_parent_list.Top().name == card.name) {
                    card_parent_list.Pop();
                    move_to_list.Push(card);
                    recordList.Push(card, card_parent_list, move_to_list);
                    return;
                }
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

            move_to_list = CheckTopOfTabelaeu(find_card_tabeleau_name1);
            if(move_to_list == null) {
                move_to_list = CheckTopOfTabelaeu(find_card_tabeleau_name2);
            }
            if(move_to_list != null) {
                var card_parent_list = card.transform.parent.GetComponent<CardList>();
                var pop_list = card_parent_list.PopFrom(card);
                foreach(var p in pop_list) {
                    move_to_list.Push(p);
                }
                recordList.Push(card, card_parent_list, move_to_list);
                return;
            }

            // Special Case: King
            if(card_number == 13) {
                foreach(var t in tableau) {
                    var list = t.GetComponent<CardList>();
                    if(list.IsEmpty())  {
                        var card_parent_list = card.transform.parent.GetComponent<CardList>();
                        var pop_list = card_parent_list.PopFrom(card);
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
                    return;
                }
            }
        }

        // If we made it here the card has no moves, so we shake it 
        card.GetComponent<UpdateSprite>().SetShake();
    }
}
