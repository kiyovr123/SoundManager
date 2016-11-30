using UnityEngine;
using System.Collections;
using DG.Tweening;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
	private const int BGM_LINE_MAX = 2;
	private const int SE_LINE_MAX = 8;
	private const int VOICE_LINE_MAX = 8;

	private AudioLoop bgmAudio;
	private AudioOneShot seAudio;
	private AudioOneShot voiceAudio;

	[SerializeField]
	private GameObject bgmParent;
	[SerializeField]
	private GameObject seParent;
	[SerializeField]
	private GameObject voiceParent;
	[SerializeField]
	private AudioClip[] bgmClips;
	[SerializeField]
	private AudioClip[] seClips;
	[SerializeField]
	private AudioClip[] voiceClips;

	protected override void Awake()
	{
		base.Awake();

		// BGM
		var bgmSources = new AudioSource[ BGM_LINE_MAX ];
		for( int i = 0; i < bgmSources.Length; i++ )
		{
			bgmSources[i] = bgmParent.AddComponent<AudioSource>();
		}
		bgmAudio = new AudioLoop( bgmSources );

		// SE
		var seSources = new AudioSource[ SE_LINE_MAX ];
		for( int i = 0; i < seSources.Length; i++ )
		{
			seSources[i] = seParent.AddComponent<AudioSource>();
		}
		seAudio = new AudioOneShot( seSources );

		// Voice
		var voiceSources = new AudioSource[ VOICE_LINE_MAX ];
		for( int i = 0; i < voiceSources.Length; i++ )
		{
			voiceSources[i] = voiceParent.AddComponent<AudioSource>();
		}
		voiceAudio = new AudioOneShot( voiceSources );
	}

	/// BGM
	public void PlayBGM( int index, float fadeTime = 0f )
	{
		if( !bgmClips.IsRange(index) )
			return;

		var clip = bgmClips[index];
		bgmAudio.Play( clip, fadeTime );
	}

	public void StopBGM( float fadeTime = 0f )
	{
		bgmAudio.Stop( fadeTime );
	}

	public void VolumeBGM( float volume )
	{
		bgmAudio.Volume( volume );
	}

	/// SE
	public void PlaySE( int index )
	{
		if( !seClips.IsRange(index) )
			return;

		var clip = seClips[index];
		seAudio.Play( clip );
	}

	public void StopSE()
	{
		seAudio.Stop();
	}

	public void VolumeSE( float volume )
	{
		seAudio.Volume( volume );
	}

	/// Voice
	public void PlayVoice( int index )
	{
		if( !voiceClips.IsRange(index) )
			return;

		var clip = voiceClips[index];
		voiceAudio.Play( clip );
	}

	public void StopVoice()
	{
		voiceAudio.Stop();
	}

	public void VolumeVoice( float volume )
	{
		voiceAudio.Volume( volume );
	}
}

public class AudioLoop
{
	private readonly AudioSource[] audioSources;
	private float volume;
	private AudioSource currentAudioSource;

	public AudioLoop( AudioSource[] audioSources )
	{
		this.audioSources = audioSources;
	}

	public void Play( AudioClip audioClip, float fadeTime )
	{
		// フェードアウト
		if( currentAudioSource != null && currentAudioSource.isPlaying )
		{
			// 鳴り直しを行わない
			if( currentAudioSource.clip == audioClip )
				return;
			
			Stop( fadeTime );
		}

		// 再生中ではないAudioSourceの取得
		var audioSource = System.Array.Find( audioSources, source => source != currentAudioSource );
		if( audioSource == null )
		{
			Debug.LogWarning( "sound not play warning!" );
			return;
		}

		// 設定
		audioSource.Stop();
		audioSource.clip = audioClip;
		audioSource.volume = 0f;
		audioSource.loop = true;
		audioSource.Play();

		// フェードイン
		DOTween.To(
			() => audioSource.volume,
			v => audioSource.volume = v,
			this.volume,
			fadeTime
		);

		currentAudioSource = audioSource;
	}

	public void Stop( float fadeTime )
	{
		if( currentAudioSource == null )
			return;
		
		// キャプチャされるので変数に逃している
		var source = currentAudioSource;
		DOTween.To(
			() => source.volume,
			v => source.volume = v,
			0f,
			fadeTime
		).OnComplete( () => source.Stop() );
	}

	public void Volume( float volume )
	{
		this.volume = volume;
		if( currentAudioSource == null )
			return;
		
		currentAudioSource.volume = volume;
	}
}

public class AudioOneShot
{
	private readonly AudioSource[] audioSources;
	private float volume;

	public AudioOneShot( AudioSource[] audioSources )
	{
		this.audioSources = audioSources;
	}

	public void Play( AudioClip audioClip )
	{
		// 再生中ではないAudioSourceの取得
		var audioSource = System.Array.Find( audioSources, source => !source.isPlaying );
		if( audioSource == null )
		{
			Debug.LogWarning( "sound not play warning!" );
			return;
		}

		// 設定
		audioSource.Stop();
		audioSource.volume = volume;
		audioSource.PlayOneShot( audioClip );
	}

	public void Stop()
	{
		System.Array.ForEach( audioSources, source =>
		{
			source.Stop();
			source.clip = null;
		} );
	}

	public void Volume( float volume )
	{
		this.volume = volume;
		System.Array.ForEach( audioSources, source =>
		{
			source.volume = volume;
		} );
	}
}
