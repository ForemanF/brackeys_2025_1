using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpUtilities : MonoBehaviour
{
    public static IEnumerator LerpToPosition(Transform obj_tf, Vector3 destination, float seconds) {
        float progress = 0;
        float time_start = Time.time;

        Vector3 starting_pos = obj_tf.position;
        while(progress < 1) {
            float time_elapsed = Time.time - time_start;

            progress = time_elapsed / seconds;
            obj_tf.position = Vector3.Lerp(starting_pos, destination, progress);

            yield return null;
        }

        obj_tf.position = destination;
    }
}
