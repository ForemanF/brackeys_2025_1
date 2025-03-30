using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    float speed = 5f;

    [SerializeField]
    Camera my_camera;

    [SerializeField]
    float zoom_intensity = 5;


    // Update is called once per frame
    void Update()
    {
        float vert = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float horiz = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float zoom = Input.GetAxis("Mouse ScrollWheel");

        Vector3 zoom_amt = my_camera.transform.forward * zoom * Time.deltaTime * zoom_intensity;

        Vector3 pos_change = new Vector3(horiz, 0, vert) + zoom_amt;
        my_camera.transform.position += pos_change;
    }
}
