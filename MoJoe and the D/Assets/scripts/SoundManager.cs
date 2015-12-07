﻿using UnityEngine;
using System;

public class SoundManager : MonoBehaviour 
{
    // Sounds which are different for each player
    public enum PlayerSoundType
    {
        Attack,
        Die,
        Jump,
        Land,
        Bump
    }

    // Sounds which are non-player specific
    public enum SoundType
    {
        Rock,
        Paper,
        Scissors,
        Step,
        MonsterBwargh,
        MonsterNom
    }

    [Serializable]
    public struct AudioClipInfo
    {
        public AudioClip clip;
        public float minVolume;
        public float maxVolume;
    }

    // Unity won't serialise a 2D array, so I have to do this shit
    [Serializable]
    public struct AudioClipList
    {
        public AudioClipInfo[] items;
    }

    [Serializable]
    public struct PlayerSoundList
    {
        public PlayerSoundType soundType;
        public AudioClipList[] clipLists;
    }

    [Serializable]
    public struct SoundList
    {
        public SoundType soundType;
        public AudioClipInfo[] clips;
    }

    [SerializeField]
    private PlayerSoundList[] playerSounds;

    [SerializeField]
    private SoundList[] sounds;

    private static SoundManager instance;
    public static SoundManager Instance { get { return instance; } }

    private void Start()
    {
        SoundManager.instance = this;

        int numSoundTypes = Enum.GetValues(typeof(SoundType)).Length;
        if (this.sounds.Length != numSoundTypes)
        {
            Array.Resize(ref this.sounds, numSoundTypes);
        }

        for (int i = 0; i < this.sounds.Length; ++i)
        {
            SoundList soundList = this.sounds[i];

            if ((int)soundList.soundType != i)
            {
                this.sounds[i] = this.sounds[(int)soundList.soundType];
                this.sounds[(int)soundList.soundType] = soundList;
            }
        }

        numSoundTypes = Enum.GetValues(typeof(PlayerSoundType)).Length;
        if (this.playerSounds.Length != numSoundTypes)
        {
            Array.Resize(ref this.playerSounds, numSoundTypes);
        }

        for (int i = 0; i < this.playerSounds.Length; ++i)
        {
            PlayerSoundList soundList = this.playerSounds[i];

            if ((int)soundList.soundType != i)
            {
                this.playerSounds[i] = this.playerSounds[(int)soundList.soundType];
                this.playerSounds[(int)soundList.soundType] = soundList;
            }
        }
    }

    public void CreateSound(SoundType type, Vector3 position)
    {
        SoundList soundList = this.sounds[(int)type];

        if (soundList.clips == null || soundList.clips.Length == 0)
        {
            Debug.LogWarning("No clips for " + type);
            return;
        }

        AudioClipInfo clipInfo = soundList.clips[UnityEngine.Random.Range(0, soundList.clips.Length)];

        CreateSound(position, clipInfo.clip, UnityEngine.Random.Range(clipInfo.minVolume, clipInfo.maxVolume));
    }

    public void CreateSound(PlayerSoundType type, int playerNum, Vector3 position)
    {
        PlayerSoundList playerSoundList = this.playerSounds[(int)type];

        if (playerSoundList.clipLists == null || playerSoundList.clipLists.Length == 0)
        {
            Debug.LogWarning("No clips for " + type);
            return;
        }

        // Get list of sounds for this playerNum
        if(playerNum > playerSoundList.clipLists.Length)
        {
            Debug.LogWarning("playerNum " + playerNum + " is too high");
            playerNum %= playerSoundList.clipLists.Length;
        }
        AudioClipList clipList = playerSoundList.clipLists[playerNum];

        AudioClipInfo clipInfo = clipList.items[UnityEngine.Random.Range(0, clipList.items.Length)];

        CreateSound(position, clipInfo.clip, UnityEngine.Random.Range(clipInfo.minVolume, clipInfo.maxVolume));
    }

    private void CreateSound(Vector3 position, AudioClip clip, float volume)
    {
        GameObject gameObject = new GameObject(clip.name);
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(clip, volume);
        gameObject.AddComponent<DestroyWhenClipFinishedPlaying>();
    }
}
