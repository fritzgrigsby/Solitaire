using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecordList
{
    List<MoveRecord> records = new List<MoveRecord>();
    [SerializeField] bool logRecord = false;

    public void Push(GameObject move_card, CardList from_slot, CardList to_slot) {
        records.Add(new MoveRecord(move_card, from_slot, to_slot));
        if(logRecord) {
            Debug.Log("\n===== Push =====");
            PrintRecords();
        }

    } 

    public MoveRecord Pop() {
        if(records.Count != 0 ) {
            var ret = records.Last();
            records.RemoveAt(records.Count - 1);
            if(logRecord) {
                Debug.Log("\n===== Pop =====");
                PrintRecords();
            }
            return ret;
        }
        return new MoveRecord(null,null,null);
    }

    public bool IsEmpty() { return records.Count == 0; }

    public struct MoveRecord 
    {
        public MoveRecord(GameObject move_card, CardList from_slot, CardList to_slot) {
            card = move_card; fromSlot = from_slot; toSlot = to_slot;
        }
        public GameObject card;
        public CardList fromSlot;
        public CardList toSlot;
    }

    void PrintRecords() {
        foreach(var r in records) {
            Debug.Log("Card: " + r.card.name + 
                      " From: " +  r.fromSlot.gameObject.name + 
                      " To: " + r.toSlot.gameObject.name);
        }
    }
}
