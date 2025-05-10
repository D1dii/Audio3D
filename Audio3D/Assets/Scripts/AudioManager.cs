using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("3D Sounds")]
    [SerializeField] private AudioSource radioSource;
    [SerializeField] private List<AudioClip> radioClips;

    [Header("Trigger Sounds")]
    [SerializeField] private AudioSource toasterSource;
    [SerializeField] private SphereCollider toasterTrigger;
    [SerializeField] private AudioSource WCSource;
    [SerializeField] private BoxCollider WCTrigger;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
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

    public void PlayToaster()
    {
        toasterSource.Play();
        toasterTrigger.enabled = false;
    }

    public void PlayWC()
    {
        WCSource.Play();
        WCTrigger.enabled = false;
        StartCoroutine(EnableWCTrigger());
    }

    public IEnumerator EnableWCTrigger()
    {
        yield return new WaitForSeconds(5f);
        WCTrigger.enabled = true;
    }
}
