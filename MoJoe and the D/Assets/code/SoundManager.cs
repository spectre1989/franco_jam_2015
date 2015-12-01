using UnityEngine;
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
    public struct PlayerSoundList
    {
        public PlayerSoundType soundType;
        public AudioClip[][] clips;
    }

    [Serializable]
    public struct SoundList
    {
        public SoundType soundType;
        public AudioClip[] clips;
    }

    [SerializeField]
    private PlayerSoundList[] playerSounds;

    [SerializeField]
    private SoundList[] sounds;
}
