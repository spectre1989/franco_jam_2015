using UnityEngine;

public class DestroyWhenClipFinishedPlaying : MonoBehaviour 
{
    private void Update()
    {
        if (this.GetComponent<AudioSource>().isPlaying == false)
        {
            Destroy(this.gameObject);
        }
    }
}
