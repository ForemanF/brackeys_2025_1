using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField]
    GameObject pause_menu;

    bool is_paused = false;

    // Start is called before the first frame update
    void Start()
    {
        pause_menu.SetActive(is_paused);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }


    public void LoadMainMenu() {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }


    public void TogglePause() {
        is_paused = !is_paused;

        if(is_paused) {
            Time.timeScale = 0;
        }
        else {
            Time.timeScale = 1;
        }

        pause_menu.SetActive(is_paused);
    }
}
