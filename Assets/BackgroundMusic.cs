using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource music;
    public AudioClip[] musicClip;
    int musicIndex;

    private void Start()
    {
      
    }
    private void startmusic()
    {
        musicIndex = Random.Range(0, musicClip.Length);
        music.clip = musicClip[musicIndex];
        music.Play();
        StartCoroutine(playmusic());
    }
    IEnumerator playmusic()
    {
       // yield return new WaitForSeconds(musicClip[musicIndex].);
      yield return null;
        
    }
}
