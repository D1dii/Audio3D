using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    [SerializeField] private ThirdPersonController playerController;

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

    [Header("Footsteps")]
    [SerializeField] private AudioSource footStepSource;
    [SerializeField] private List<AudioClip> normalFloorFootStepClips;
    [SerializeField] private List<AudioClip> carpetFloorFootStepClips;

    [Header("Jump")]
    [SerializeField] private AudioSource jumpSource;

    private bool isJumping = false;

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

    public void PlayJumpSound()
    {
        jumpSource.Play();
        isJumping = true;
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

    public void PlayFootSteps()
    {
        if (jumpSource.isPlaying)
        {
            return; 
        }
        if (isJumping && !jumpSource.isPlaying)
        {
            isJumping = false;
            if (playerController.isMoving)
            {
                PlayFootSteps();
            }
        }
        if (playerController.floorType == ThirdPersonController.FloorType.Normal)
        {
            int randomIndex = Random.Range(0, normalFloorFootStepClips.Count);
            footStepSource.clip = normalFloorFootStepClips[randomIndex];
            footStepSource.Play();
        }
        else if (playerController.floorType == ThirdPersonController.FloorType.Carpet)
        {
            int randomIndex = Random.Range(0, carpetFloorFootStepClips.Count);
            footStepSource.clip = carpetFloorFootStepClips[randomIndex];
            footStepSource.Play();
        }
    }
}
