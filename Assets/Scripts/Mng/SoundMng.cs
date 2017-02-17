using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundMng : MonoBehaviour
{

    public static SoundMng Instance;

    private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();

    private AudioSource source;

    void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>();
        source.spatialBlend = 0f;
    }

    public void PlaySound(string path)
    {
        AudioClip clip = null;
        if (!audioCache.TryGetValue(path, out clip))
        {
            clip = Resources.Load<AudioClip>(path);
            audioCache.Add(path,clip);
        }
        
        source.PlayOneShot(clip,0.8f);
    }

    public void PlayMusic(string path,bool loop = false)
    {
        AudioClip clip = null;
        if (!audioCache.TryGetValue(path, out clip))
        {
            clip = Resources.Load<AudioClip>(path);
            audioCache.Add(path, clip);
        }

        if (source.isPlaying)
        {
            source.Stop();
        }

        source.loop = loop;
        source.clip = clip;
        source.Play();
    }

    public void StopMusic()
    {
        source.Stop();
        source.clip = null;
    }

    public void PauseMusic()
    {
        source.Pause();
    }

    public void ResumeMusic()
    {
        source.UnPause();
    }

}
