using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioType { 
    PlaceBuilding,
    Shoot,
    Heal,
    Damage,
    Click,
    FailedClick
}

[System.Serializable] public class AudioTuple { public AudioType audio_type; public AudioClip audio_clip; }

public class AudioManager : MonoBehaviour
{

    [SerializeField]
    List<AudioTuple> audio_list;

    Dictionary<AudioType, AudioClip> audio_dict;

    Subscription<AudioEvent> audio_sub;

    // Start is called before the first frame update
    void Start()
    {
        audio_dict = new Dictionary<AudioType, AudioClip>();

        foreach(AudioTuple audio_tuple in audio_list) {
            audio_dict[audio_tuple.audio_type] = audio_tuple.audio_clip;
        }

        audio_sub = EventBus.Subscribe<AudioEvent>(_OnAudioEvent);
    }

    void _OnAudioEvent(AudioEvent e) {
        Debug.Log("Playing Sound");
        AudioSource.PlayClipAtPoint(audio_dict[e.audio_type], e.position, e.volume);
    }
}
