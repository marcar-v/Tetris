using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource clickSound;
    public void PlayButton()
    {
        clickSound.Play();
    }
}
