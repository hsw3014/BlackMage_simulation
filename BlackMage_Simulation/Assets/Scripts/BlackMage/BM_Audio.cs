using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BM_Audio : MonoBehaviour
{
    // Start is called before the first frame update

    public AudioClip audio_attack;
    public AudioClip audio_creation;
    public AudioClip audio_destruction;
    AudioSource audioSource;

    void Start()
    {
        //재생본체
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //state에 따라 재생한다.
    public void BM_PlaySound(string state)
    {
        switch (state)
        {
            case "Attack":
                audioSource.clip = audio_attack;
                break;
            case "Power_Creation":
                audioSource.clip = audio_creation;
                break;
            case "Power_Destruction":
                audioSource.clip = audio_destruction;
                break;
        }

        //지정된 오디오 재생
        audioSource.Play();
    }
}
