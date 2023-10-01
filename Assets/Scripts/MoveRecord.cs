using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class RecordList
{
    List<MoveRecord> records = new List<MoveRecord>();

    public void Push(GameObject move_card, CardList from_slot, CardList to_slot) {
        records.Add(new MoveRecord(move_card, from_slot, to_slot));
    } 

    public MoveRecord Pop() {
        if(records.Count != 0 ) {
            var ret = records.Last();
            records.Remove(ret);
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

}
