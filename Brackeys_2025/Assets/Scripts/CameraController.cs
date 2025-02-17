using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    float speed = 5f;

    [SerializeField]
    Camera my_camera;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float vert = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float horiz = Input.GetAxis("Horizontal") * speed * Time.deltaTime;

        Vector3 pos_change = new Vector3(horiz, 0, vert);
        my_camera.transform.position += pos_change;
    }
}
