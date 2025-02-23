using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Faction { 
    Player,
    Enemy
}

public class HasFaction : MonoBehaviour
{

    [SerializeField]
    Faction my_faction = Faction.Enemy;

    public Faction GetFaction() {
        return my_faction;
    }
}
