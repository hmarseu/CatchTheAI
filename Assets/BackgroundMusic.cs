using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource music;
    public AudioClip[] musicClip;


    private void Start()
    {
        
    }
    IEnumerator playmusic()
    {
        yield return null;
    }
}
