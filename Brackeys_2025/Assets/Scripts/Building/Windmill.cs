using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Windmill : Building
{
    override public IEnumerator ProcessBuildingAction()
    {
        EventBus.Publish(new DrawCardEvent(special_value)); 
        yield return null;
    }
}
