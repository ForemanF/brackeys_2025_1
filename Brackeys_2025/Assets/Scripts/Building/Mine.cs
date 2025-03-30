using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : Building
{
    override public IEnumerator ProcessBuildingAction()
    {
        EventBus.Publish(new IncreaseEnergyEvent(special_value));
        yield return null;
    }
    
}
