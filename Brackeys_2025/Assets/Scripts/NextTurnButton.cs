using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnButton : MonoBehaviour
{
    [SerializeField]
    GameManager game_manager;

    Button button;


    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
    }

    public void OnNextTurnClicked() {
        if(game_manager.IsReadyForNextTurn()) { 
            EventBus.Publish(new NextTurnEvent());
            EventBus.Publish(new AudioEvent(AudioType.Click, Camera.main.transform.position));
        }
        else {
            Debug.Log("Not ready for next turn yet");
            EventBus.Publish(new AudioEvent(AudioType.FailedClick, Camera.main.transform.position));
        }
    }
}
