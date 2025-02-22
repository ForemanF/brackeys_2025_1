using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpacityFader : MonoBehaviour
{
    [SerializeField]
    float period_s = 1;

    [SerializeField]
    MeshRenderer mesh_renderer;

    Color current_color = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoopOpacity());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color new_color) {
        current_color = new_color;
    }

    IEnumerator LoopOpacity() {
        while(true) {
            float progress = 0;
            float start_time = Time.time;
            while(progress < 1) {
                progress = (Time.time - start_time) / period_s;

                float alpha = Mathf.Lerp(0, 1, progress);
                Color new_color = current_color;
                new_color.a = alpha;

                mesh_renderer.material.color = new_color;

                yield return null;
            }

            progress = 0;
            start_time = Time.time;
            while(progress < 1) {
                progress = (Time.time - start_time) / period_s;

                float alpha = Mathf.Lerp(1, 0, progress);
                Color new_color = current_color;
                new_color.a = alpha;

                mesh_renderer.material.color = new_color;

                yield return null;
            }

            yield return null;
        }
    }
}
