using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("3D Sounds")]
    [SerializeField] private AudioSource radioSource;
    [SerializeField] private List<AudioClip> radioClips;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!radioSource.isPlaying)
        {
            PlayRadio();
        }
    }

    public void PlayRadio()
    {
        int randomIndex = Random.Range(0, radioClips.Count);
        radioSource.clip = radioClips[randomIndex];
        radioSource.Play();
    }
}
