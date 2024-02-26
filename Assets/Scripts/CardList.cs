using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardList : MonoBehaviour
{
    List<GameObject> cards = new List<GameObject>();

    // Default placement offset is to stack on top of eachother with offset in z direction
    Vector3 placementOffset = new Vector3(0, 0, -0.02f);

    // Set offset for placemnt of fanned out cards 
    public void SetPlacementOffsets(float x, float y) {
        placementOffset = new Vector3(x, y, placementOffset.z);
    }

    // Push card to top of stack
    public void Push(GameObject card) {

        // Add card to list
        cards.Add(card);
        card.transform.SetParent(transform);

        // Set new position
        Vector3 new_position = transform.position + placementOffset * (cards.Count-1);
        card.GetComponent<UpdateSprite>().SetMovePosition(new_position);
    }

    // Get refrence to, and remove last/top card
    public GameObject Pop() {
        GameObject return_card = null;
        if(cards.Count != 0) {
            return_card = cards.Last();
            cards.Remove(return_card);
        }
        return return_card;
    }

    // Get refrence to last/top card
    public GameObject Top() {
        if(cards.Count != 0) {
            return cards.Last();
        }else{ return null; }
    }

    // Get refrence to, and remove first/bottom card
    public GameObject PopFront() {
        GameObject return_card = null;
        if(cards.Count != 0) {
            return_card = cards.First();
            cards.Remove(return_card);
        }
        return return_card;
    }

    // Get refrence to first/bottom card
    public GameObject Front() {
        if(cards.Count !=0) {
            return cards.First();
        }
        return null;
    }
    
    // Get refrence to first/bottom slelectable card in list
    public GameObject FirstSelectable(){
        foreach(GameObject c in cards) {
            if(c.GetComponent<Selectable>().selectable){
                return c;
            }
        }
        return null;
    }

    // Get list of cards starting from the passed in card including all the ones on top of it
    public List<GameObject> PopFrom(GameObject card) {
        var stack = new List<GameObject>();
        foreach(var c in cards) {
            if(c == card || stack.Count != 0) {
                stack.Add(c);
            }
        }
        foreach(var s in stack) {
            cards.Remove(s);
        }
        return stack;
    }

    // Shuffel stack of cards
    public void Shuffel() {
        System.Random random = new System.Random();
        for(int i=0; i<cards.Count; ++i) { 
            int r = random.Next(0, cards.Count);
            (cards[i], cards[r]) = (cards[r], cards[i]);
        }
    }
        
    public bool IsEmpty() {
        return cards.Count == 0;
    }

}
