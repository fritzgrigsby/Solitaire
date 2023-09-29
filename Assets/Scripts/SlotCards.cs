using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotCards : MonoBehaviour
{
    List<GameObject> cards = new List<GameObject>();

    public void PushCard(GameObject card) {
        cards.Add(card);
    }

    public GameObject PopCard() {
        GameObject return_card = null;
        if(cards.Count != 0) {
            return_card = cards.Last();
            cards.Remove(return_card);
        }
        return return_card;
    }

    bool IsEmpty() {
        return cards.Count == 0;
    }

}
