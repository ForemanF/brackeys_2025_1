using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stronghold : Building
{
    override public IEnumerator ProcessBuildingAction()
    {
        EventBus.Publish(new IncreaseEnergyEvent(special_value));

        EventBus.Publish(new DrawCardEvent(special_value)); 
        yield return null;
    }
}
