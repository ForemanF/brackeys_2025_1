using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lookout : Building
{
    override public IEnumerator ProcessBuildingAction()
    {
        EventBus.Publish(new RevealNearestFogEvent(current_tile, special_value));
        yield return null;
    }
}
