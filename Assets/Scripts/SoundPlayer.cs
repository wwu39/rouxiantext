using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VibDepth
{
    no,
    shallow,
    deep
}
public enum VibRate
{
    slow,
    fast
}

public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer ins;
    [FMODUnity.EventRef]
    public string vEvent;
    FMOD.Studio.EventInstance vEventIns;
    private void Awake()
    {
        ins = this;
    }
    void Start()
    {
        vEventIns = FMODUnity.RuntimeManager.CreateInstance(vEvent);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(vEventIns, transform, GetComponent<Rigidbody>());
        vEventIns.start();
    }
    public static void SetVibDepth(VibDepth d)
    {
        ins.vEventIns.setParameterByName("vib_depth", (int)d);
    }
    public static void SetVibRate(VibRate r)
    {
        ins.vEventIns.setParameterByName("vib_rate", (int)r);
    }
    public static void SetVolume(float v)
    {
        ins.vEventIns.setVolume(v);
    }
}
