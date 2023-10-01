using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class CardList : MonoBehaviour
{
    List<GameObject> cards = new List<GameObject>();

    Vector3 placementOffset = new Vector3(0, 0, -0.02f);

    public void SetPlacementOffsets(float x, float y) {
        placementOffset = new Vector3(x, y, placementOffset.z);
    }

    public void Push(GameObject card) {

        // Add card to list
        cards.Add(card);
        card.transform.SetParent(transform);

        // Set new position
        Vector3 new_position = transform.position + placementOffset * (cards.Count-1);
        card.GetComponent<UpdateSprite>().SetMovePosition(new_position);
    }

    public GameObject Pop() {
        GameObject return_card = null;
        if(cards.Count != 0) {
            return_card = cards.Last();
            cards.Remove(return_card);
        }
        return return_card;
    }

    public GameObject Top() {
        if(cards.Count != 0) {
            return cards.Last();
        }else{ return null; }
    }

    public GameObject PopFront() {
        GameObject return_card = null;
        if(cards.Count != 0) {
            return_card = cards.First();
            cards.Remove(return_card);
        }
        return return_card;
    }

    public GameObject Front() {
        if(cards.Count !=0) {
            return cards.First();
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
