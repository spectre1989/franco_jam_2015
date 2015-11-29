using UnityEngine;

public class OneShotAudioClip : MonoBehaviour 
{
    public static void Create(Vector3 position, AudioClip clip)
    {
        GameObject go = new GameObject(clip.name);
        go.transform.position = position;
        go.AddComponent<AudioSource>().PlayOneShot(clip);
        go.AddComponent<OneShotAudioClip>();
    }

    private void Update()
    {
        if (this.GetComponent<AudioSource>().isPlaying == false)
        {
            Destroy(this.gameObject);
        }
    }
}
