using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CardButton_SO", order = 1)]
public class CardButton_SO : ScriptableObject
{
    public new string name; 
    public Sprite cardSprite;
    public Sprite hilightedSprite;
}
