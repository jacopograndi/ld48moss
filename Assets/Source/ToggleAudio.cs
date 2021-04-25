using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleAudio : MonoBehaviour {

    public bool toggle = true;
    AudioSource[] sources;

    private void Start() {
        sources = FindObjectsOfType<AudioSource>();
    }

    public void Toggle () {
        toggle = !toggle;
        foreach (AudioSource src in sources) {
            if (toggle) src.volume = 0.1f;
            else src.volume = 0;
        }
    }
}
