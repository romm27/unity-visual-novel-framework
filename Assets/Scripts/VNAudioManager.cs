using System.Collections.Generic;
using UnityEngine;

public class VNAudioManager : MonoBehaviour
{
    [Header("References")]
    public ActiveAudio[] fixedChannel = new ActiveAudio[6]; //0 - music1; 1 - music2; 2 - ambient1; 3 - ambient2; 4 - voice1; 5 - voice2
    public List<ActiveAudio> audioTree = new List<ActiveAudio>();
    [SerializeField] private GameObject fixedChannelsRoot;
    [SerializeField] private GameObject dynamicChannelsRoot;

    [Header("Debug")]
    public bool ErrorOnFailedSoundLoad = false;

    public string CurrentLoopingMusic {
        get {
            return fixedChannel[0].source.clip?.name;
        }
    }

    public string CurrentLoopingAmbiance {
        get {
            return fixedChannel[2].source.clip?.name;
        }
    }

    public void Awake() {
        for (int i = 0; i < fixedChannel.Length; i++) {
            fixedChannel[i].source = fixedChannelsRoot.AddComponent<AudioSource>();
            fixedChannel[i].source.loop = i < 4; //set all fixed channels but the voice ones to loop
        }

        //UpdateVolume();
    }

    public void Start() {
        UpdateVolume();
    }

    //Methods
    public void PlayDynamicAudio(AudioClip _clip, float _volume = 1f) {
        if(_clip == null) {
            //Debug.LogWarning("Tried to play null dynamic audio!");
            return;
        }

        int channel = RetrieveNewActiveAudioIndex(_clip.name);


        audioTree[channel].source.clip = _clip;
        audioTree[channel].source.volume = _volume;
        audioTree[channel].source.Play();
    }

    public void PlayDynamicAudio(string _clipName, float _volume = 1f) {
        PlayDynamicAudio(GetAudio(_clipName, 0), _volume);
    }


    public int RetrieveNewActiveAudioIndex(string _audioName) {
        for (int i = 0; i < audioTree.Count; i++) {
            if (!audioTree[i].IsPlaying) {
                return i;
            }
        }

        int newId = audioTree.Count + 1;
        audioTree.Add(new ActiveAudio(newId, _audioName, dynamicChannelsRoot.AddComponent<AudioSource>()));
        return audioTree.Count - 1;
    } 

    public AudioClip GetAudioFromList(string _identifier, AudioClip[] _audioClipList) {
        if (ErrorOnFailedSoundLoad && _audioClipList.Length == 0) {
            Debug.LogError("Sound list probably failed to load!");
            return null;
        }

        for (int i = 0; i < _audioClipList.Length; i++) {
            if(_identifier == _audioClipList[i].name) {
                return _audioClipList[i];
            }
        }

        Debug.LogWarning("No sound file find named " + _identifier);
        return null;
    }

    public AudioClip GetAudio(string _identifier, int _type) { //0 - effect, 1 - music
        switch (_type) {
            case 0:
                return GetAudioFromList(_identifier, VNDirector.instance.ActiveArcDataPack.sceneAudioEffects);
            case 1:
                return GetAudioFromList(_identifier, VNDirector.instance.ActiveArcDataPack.sceneAudioLoops);
            default:
                Debug.LogError("Invalid Audio Type");
                return null;
        }
    
    }

    public enum SoundCommandType { Music, Ambient, Effect, Voice }

    public void SetAudioForChannel(AudioClip _clip, SoundCommandType _channel) {
        int channel = 0;

        switch(_channel) {
            case SoundCommandType.Music:
                channel = 0;
                break;
            case SoundCommandType.Ambient:
                channel = 2;
                break;
            case SoundCommandType.Voice:
                channel = 4;
                break;
        }

        PlayInChannel(_clip, channel);
    }

    public void SetAudioForChannel(string _clipName, SoundCommandType _channel) {
        SetAudioForChannel(GetAudio(_clipName, 1), _channel);
    }

    public void PlayInChannel(AudioClip _clip, int _channel) {
        fixedChannel[_channel].source.clip = _clip;
        fixedChannel[_channel].source.Play();
    }

    public void SilenceAudioForChannel(int _channel) {
        fixedChannel[_channel].source.Pause();
    }

    public void SilenceAudioForChannel(SoundCommandType _soundCommandType) {
        int channel = -1;

        switch (_soundCommandType) {
            case SoundCommandType.Music:
                channel = 0;
                break;
            case SoundCommandType.Ambient:
                channel = 2;
                break;
            case SoundCommandType.Voice:
                channel = 4;
                break;
            default:
                Debug.LogWarning("Can't mute dynamic sound effect!");
                break;
        }

        if (channel >= 0) {
            fixedChannel[channel].source.Pause();
        }
    }

    public void UpdateVolume() {
        fixedChannel[0].source.volume = VNDirector.instance.globalOptions.musicVolume * VNDirector.instance.globalOptions.mainVolume;
        fixedChannel[1].source.volume = VNDirector.instance.globalOptions.musicVolume * VNDirector.instance.globalOptions.mainVolume;
        fixedChannel[2].source.volume = VNDirector.instance.globalOptions.ambientVolume * VNDirector.instance.globalOptions.mainVolume;
        fixedChannel[3].source.volume = VNDirector.instance.globalOptions.ambientVolume * VNDirector.instance.globalOptions.mainVolume;
        fixedChannel[4].source.volume = VNDirector.instance.globalOptions.voiceVolume * VNDirector.instance.globalOptions.mainVolume;
        fixedChannel[5].source.volume = VNDirector.instance.globalOptions.voiceVolume * VNDirector.instance.globalOptions.mainVolume;
    }
}

[System.Serializable]
public struct ActiveAudio
{
    public int audioId;
    public string audioName;
    public AudioSource source;

    public bool IsLoop {
        get {
            return source.loop;
        }
    }

    public bool IsPlaying {
        get {
            return source.isPlaying;
        }
    }

    public ActiveAudio(int _audioId, string _fileName, AudioSource _source) {
        audioId = _audioId;
        audioName = _fileName;
        source = _source;
    }
}
