using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// 声音模块
/// 用于游戏中的所有声音播放
/// </summary>
public class Audio : BaseModule {

    private Transform audioListenerTrans = null;

    private int audioIdIndex = 0;
    /// <summary>
    /// 这个字典不知道要用来干嘛
    /// </summary>
    private Dictionary<int, _InternalAudioObject> audioDict = new Dictionary<int, _InternalAudioObject> ();

    /// <summary>
    /// 声音播放器缓存池
    /// </summary>
    private Queue<_InternalAudioObject> audioObjectCache = new Queue<_InternalAudioObject>();

    /// <summary>
    /// 当前正在播放的声音
    /// </summary>
    private List<_InternalAudioObject> playingAudioList = new List<_InternalAudioObject>();


    public override void Initialize() {
        base.Initialize();
    }


    /// <summary>
    /// 设置AudioListener所在物体的Transform，调用Play的时候如果没有指定播放 [位置] 或 [父物体] 则默认在AudioListener的transform下播放
    /// 如果没有指定 AudioListenerTransform 则会默认在 （0，0，0）的位置播放
    /// 友情提示：最好不要直接把AudioListener挂在会移动的Camera上，我发现音频在移动的Camera下播放时有时候会不正常
    ///          最好的做法是这样，建立一个空物体 Root, Camera 和 挂载AudioListener的物体都在Root下面，移动的时候移动Root，这样就不会有问题
    /// </summary>
    /// <param name="_audioListenerTrans"></param>
    public void SetAudioListener(Transform _audioListenerTrans) {
        audioListenerTrans = _audioListenerTrans;
    }


    public override void OnUpdate() {
        base.OnUpdate();

        int count = playingAudioList.Count;

        for(int i = count - 1; i >= 0; --i) {
            _InternalAudioObject internalAudioObj = playingAudioList[i];

            // Holder 被用户删除，这个 InternalAudioObject 报废
            if(internalAudioObj.holderObj == null) {
                playingAudioList.RemoveAt(i);
                continue;
            }

            AudioSource audioSource = internalAudioObj.audioSource;
            if(!audioSource.loop && !audioSource.isPlaying) {
                playingAudioList.RemoveAt(i);
                CacheInternalAudioObject(internalAudioObj);
            }
        }
    }

    /// <summary>
    /// 在 AudioListener 下播放音频
    /// </summary>
    /// <param name="_audioId"></param>
    /// <returns></returns>
    public AudioObject PlayAudio (int _audioId) {
        _InternalAudioObject iAudioObj = InternalPlayAudio(_audioId);
        if(iAudioObj != null) {
            iAudioObj.PlayAtTransform(audioListenerTrans);
            playingAudioList.Add(iAudioObj);
            return new AudioObject(iAudioObj.id);
        }

        return null;
    }

    /// <summary>
    /// 在 AudioListener 下播放多次音频
    /// </summary>
    /// <param name="_audioId"></param>
    /// <param name="_playTimes">播放次数</param>
    /// <returns></returns>
    public AudioObject PlayAudio (int _audioId, int _playTimes) {
        _InternalAudioObject iAudioObj = InternalPlayAudio(_audioId);

        if(iAudioObj != null) {
            iAudioObj.isPlayMultiTimes = true;
            iAudioObj.maxPlayTimes = _playTimes;
            iAudioObj.PlayAtTransform(audioListenerTrans);
            playingAudioList.Add(iAudioObj);
            return new AudioObject(iAudioObj.id);
        }
        return null;
    }

    /// <summary>
    /// 在 AudioListener 下延迟播放音频
    /// </summary>
    /// <param name="_audioId"></param>
    /// <param name="_delaySeconds">延迟秒数</param>
    /// <returns></returns>
    public AudioObject PlayAudio (int _audioId, float _delaySeconds) {
        _InternalAudioObject iAudioObj = InternalPlayAudio(_audioId);
        if(iAudioObj != null) {
            iAudioObj.isDelay = true;
            iAudioObj.delayTicker = _delaySeconds;
            iAudioObj.PlayAtTransform(audioListenerTrans);
            playingAudioList.Add(iAudioObj);
            return new AudioObject(iAudioObj.id);
        }
        return null;
    }

    /// <summary>
    /// 在 AudioListener 下延迟播放多次音频
    /// </summary>
    /// <param name="_audioId"></param>
    /// <param name="_delaySeconds">延迟秒数</param>
    /// <param name="_playTimes">播放次数</param>
    /// <returns></returns>
    public AudioObject PlayAudio (int _audioId, float _delaySeconds, int _playTimes) {
        _InternalAudioObject iAudioObj = InternalPlayAudio(_audioId);
        if(iAudioObj != null) {
            iAudioObj.isDelay = true;
            iAudioObj.delaySeconds = _delaySeconds;
            iAudioObj.isPlayMultiTimes = true;
            iAudioObj.maxPlayTimes = _playTimes;
            iAudioObj.PlayAtTransform(audioListenerTrans);
            playingAudioList.Add(iAudioObj);
            return new AudioObject(iAudioObj.id);
        }
        return null; 
    }

    /// <summary>
    /// 在指定位置播放音频
    /// </summary>
    /// <param name="_audioId"></param>
    /// <param name="_worldPos">指定的位置</param>
    /// <returns></returns>
    public AudioObject PlayAudio (int _audioId, Vector3 _worldPos) {
        _InternalAudioObject iAudioObj = InternalPlayAudio(_audioId);
        if(iAudioObj != null) {
            iAudioObj.PlayAtWorldPos(_worldPos);
            playingAudioList.Add(iAudioObj);
            return new AudioObject(iAudioObj.id);
        }
        return null;
    }

    /// <summary>
    /// 在指定的位置播放多次音频
    /// </summary>
    /// <param name="_audioId"></param>
    /// <param name="_worldPos">播放位置</param>
    /// <param name="_playTimes">播放次数</param>
    /// <returns></returns>
    public AudioObject PlayAudio (int _audioId, Vector3 _worldPos, int _playTimes) {
        _InternalAudioObject iAudioObj = InternalPlayAudio(_audioId);
        if(iAudioObj != null) {
            iAudioObj.isPlayMultiTimes = true;
            iAudioObj.maxPlayTimes = _playTimes;
            iAudioObj.PlayAtWorldPos(_worldPos);
            playingAudioList.Add(iAudioObj);
            return new AudioObject(iAudioObj.id);
        }
        return null; 
    }


    /// <summary>
    /// 在指定位置延迟播放视频
    /// </summary>
    /// <param name="_audioId"></param>
    /// <param name="_worldPos">播放位置</param>
    /// <param name="_delaySeconds">延迟秒数</param>
    /// <returns></returns>
    public AudioObject PlayAudio (int _audioId, Vector3 _worldPos, float _delaySeconds) {
        _InternalAudioObject iAudioObj = InternalPlayAudio(_audioId);
        if( iAudioObj != null) {
            iAudioObj.isDelay = true;
            iAudioObj.delaySeconds = _delaySeconds;
            iAudioObj.PlayAtWorldPos(_worldPos);
            playingAudioList.Add(iAudioObj);
            return new AudioObject(iAudioObj.id);
        }
        return null;

          

    } 

    /// <summary>
    /// 在指定的位置延迟播放多次音频
    /// </summary>
    /// <param name="_audioId"></param>
    /// <param name="_worldPos">播放位置</param>
    /// <param name="_delaySeconds">延迟时间</param>
    /// <param name="_playTimes">播放次数</param>
    /// <returns></returns>
    public AudioObject PlayAudio (int _audioId, Vector3 _worldPos, float _delaySeconds, int _playTimes) {
        _InternalAudioObject iAudioObj = InternalPlayAudio(_audioId);
        if(iAudioObj != null) {
            iAudioObj.isDelay = true;
            iAudioObj.delaySeconds = _delaySeconds;
            iAudioObj.isPlayMultiTimes = true;
            iAudioObj.maxPlayTimes = _playTimes;
            iAudioObj.PlayAtWorldPos(_worldPos);
            playingAudioList.Add(iAudioObj);
            return new AudioObject(iAudioObj.id);
        }
        return null;
    }

    public AudioObject PlayAudio (int _audioId, Transform _parent) {
        return null;
    }

    public AudioObject PlayAudio (int _audioId, Transform _parent, int _playTimes) {
        return null; 
    }

    public AudioObject PlayAudio (int _audioId, Transform _parent, float _delaySeconds) {
        return null; 
    }

    public AudioObject PlayAudio (int _audioId, Transform _parent, float _delaySeconds, int _playTimes) {
        return null;
    }

    public void StopAudio (AudioObject _audioObj) {

    }

    public void SetMute (AudioObject _audioObj, bool _mute) {

    }

    public bool IsMute (AudioObject _audioObj) {
        return false;
    }

    public void SetBypassEffects (AudioObject _audioObj, bool _value) {

    }

    public bool IsBypassEffects (AudioObject _audioObj, bool _value) {
        return false;
    }

    public void SetBypassListenerEffect (AudioObject _audioObj, bool _value) {

    }

    public bool IsBypassListenerEffect (AudioObject _audioObj) {
        return false;
    }

    public void SetBypassReverbZones (AudioObject _audioObj, bool _value) {

    }

    public bool IsBypassReverbZones (AudioObject _audioObj) {
        return false;
    }

    public void SetPlayOnAwake (AudioObject _audioObj, bool _value) {

    }

    public bool IsPlayOnAwake (AudioObject _audioObj) {
        return false;
    }

    public void SetLoop (AudioObject _audioObj, bool _value) {

    }

    public bool IsLoop (AudioObject _audioObj) {
        return false;
    }

    public void SetPriority (AudioObject _audioObj, int _value) {

    }

    public int GetPriority (AudioObject _audioObj) {
        return 0;
    }

    /// <summary>
    /// Sets the volume.
    /// </summary>
    /// <param name="_audioObj">Audio object.</param>
    /// <param name="_value">Value. Max 1</param>
    public void SetVolume (AudioObject _audioObj, float _value) {

    }

    public float GetVolume (AudioObject _audioObj) {
        return 0.0f;
    }

    /// <summary>
    /// Sets the pitch.
    /// </summary>
    /// <param name="_audioObj">Audio object.</param>
    /// <param name="_value">Value. Max 3</param>
    public void SetPitch (AudioObject _audioObj, float _value) {

    }

    public float GetPitch (AudioObject _audioObj) {
        return 0.0f;
    }

    /// <summary>
    /// Sets the stereo pan.
    /// </summary>
    /// <param name="_audioObj">Audio object.</param>
    /// <param name="_value">Value. Min -1.0f  Max 1.0f </param>
    public void SetStereoPan (AudioObject _audioObj, float _value) {

    }

    public float GetStereoPan (AudioObject _audioObj) {
        return 0.0f;
    }

    /// <summary>
    /// Sets the spatial blend.
    /// </summary>
    /// <param name="_audioObj">Audio object.</param>
    /// <param name="_value">Value. Max 1.0f</param>
    public void SetSpatialBlend (AudioObject _audioObj, float _value) {

    }

    public float GetSpatialBlend (AudioObject _audioObj) {
        return 0.0f;
    }

    /// <summary>
    /// Sets the reverb zone mix.
    /// </summary>
    /// <param name="_audioObj">Audio object.</param>
    /// <param name="_value">Value. Min 0.0f  Max 1.1f </param>
    public void SetReverbZoneMix (AudioObject _audioObj, float _value) {

    }

    public float GetReverbZoneMix (AudioObject _audioObj) {
        return 0.0f;
    }

    /// <summary>
    /// Sets the dopple leverl.
    /// </summary>
    /// <param name="_audioObj">Audio object.</param>
    /// <param name="_value">Value. Min 0.0f  Max 5.0f</param>
    public void SetDoppleLeverl (AudioObject _audioObj, float _value) {

    }

    public float GetDoppleLevel (AudioObject _audioObj) {
        return 0.0f;
    }


    /// <summary>
    /// Sets the spread.
    /// </summary>
    /// <param name="_audioObj">Audio object.</param>
    /// <param name="_value">Value. Min 0  Max 360</param>
    public void SetSpread (AudioObject _audioObj, int _value) {

    }

    public int GetSpread (AudioObject _audioObj) {
        return 0;
    }


    public void SetVolumeRolloff (AudioObject _audioObj, AudioRolloffMode _mode) {

    }

    public AudioRolloffMode GetVolumeRolloff (AudioObject _audioObj) {
        return AudioRolloffMode.Linear;
    }


    public void SetMinDistance (AudioObject _audioObj, float _value) {

    }

    public float GetMinDistance (AudioObject _audioObj) {
        return 0.0f;
    }

    public void SetMaxDistance (AudioObject _audioObj, float _value) {

    }

    public float GetMaxDistance (AudioObject _audioObj) {
        return 0.0f;
    }



    private void DefaultAudioClip (AudioClip _clip) {
        
    }


    /// <summary>
    /// 创建一个音频播放对象
    /// </summary>
    /// <param name="_audioId"></param>
    /// <returns></returns>
    private _InternalAudioObject InternalPlayAudio(int _audioId) {

        AudioClip clip = V.vResource.GetAsset<AudioClip>(_audioId);
        if(clip == null) {
            Ulog.LogError("找不到要播放的声音: ", _audioId);
            return null;
        }

        _InternalAudioObject iAudioObj = GetInternalAudioObjectFromCache();

        if (iAudioObj == null) {
            iAudioObj = new _InternalAudioObject();
            iAudioObj.id = ++audioIdIndex;
            iAudioObj.audioId = _audioId;
            iAudioObj.holderObj = new GameObject("Audio:" + _audioId);
            iAudioObj.audioSource = iAudioObj.holderObj.AddComponent<AudioSource>();
        } 

        iAudioObj.audioSource.clip = clip;

        return iAudioObj;
    }


    private _InternalAudioObject GetInternalAudioObjectFromCache() { 
        if(audioObjectCache.Count > 0) {
            return audioObjectCache.Dequeue();
        }
        return null;
    }

    private void CacheInternalAudioObject(_InternalAudioObject _internalAudioObj) {
        // Holder被用户删除，这个 InternalGameObject 报废，愚蠢的用户
        if(_internalAudioObj.holderObj == null) {
            return;
        }
        _internalAudioObj.audioSource.Stop();

        // 重置数据
        _internalAudioObj.ResetIAudioObject();

        audioObjectCache.Enqueue(_internalAudioObj);
    }


    internal class _InternalAudioObject {
        public int id;
        public int audioId;
        public GameObject holderObj;
        public AudioSource audioSource;
        public AudioClip audioClip;
        public bool isDelay = false;
        public float delaySeconds = 0.0f;
        public float delayTicker = 0.0f;
        public bool isPlayMultiTimes = false;
        public int maxPlayTimes = 0;
        public int timesTicker = 0;

        public void Play() {
            ++timesTicker;
            audioSource.Play();
        }

        /// <summary>
        /// 在指定位置播放
        /// </summary>
        /// <param name="_worldPos"></param>
        public void PlayAtWorldPos(Vector3 _worldPos) {
            ++timesTicker;
            holderObj.transform.position = _worldPos;
        }

        /// <summary>
        /// 在指定的父物体下播放
        /// </summary>
        /// <param name="_parent"></param>
        public void PlayAtTransform(Transform _parent) {
            ++timesTicker;
            if (_parent != null) {
                holderObj.transform.SetParent(_parent);
                holderObj.transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public void ResetIAudioObject() {
            holderObj.transform.SetParent(null);
            holderObj.transform.position = Vector3.zero;
            isDelay = false;
            delaySeconds = 0.0f;
            delayTicker = 0.0f;
            isPlayMultiTimes = false;
            maxPlayTimes = 0;
            timesTicker = 0;
            ResetAudioSourceToDefault();
        }

        private void ResetAudioSourceToDefault() {
            audioSource.mute = false;
            audioSource.bypassEffects = false;
            audioSource.bypassListenerEffects = false;
            audioSource.bypassReverbZones = false;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.priority = 128;
            audioSource.volume = 1;
            audioSource.pitch = 1;
            audioSource.panStereo = 0;
            audioSource.spatialBlend = 0;
            audioSource.reverbZoneMix = 1;
            audioSource.dopplerLevel = 1;
            audioSource.spread = 0;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 1;
            audioSource.maxDistance = 500;
            
        }
    }
}



public class AudioObject {
    public int audioObjectId;

    public AudioObject(int _oId) {
        audioObjectId = _oId;
    }
}


