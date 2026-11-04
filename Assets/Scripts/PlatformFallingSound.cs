using UnityEngine;

public class PlatformFallingSound : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        AudioSource platformAudio = hit.gameObject.GetComponent<AudioSource>();

        if (platformAudio != null)
        {
           
            if (!platformAudio.isPlaying && platformAudio.enabled)
            {
                platformAudio.Play();

                var dynamicCheck = hit.gameObject.GetComponent<PlatformFallingSound>();
                if (dynamicCheck != null) Destroy(dynamicCheck);
            }
        }
    }
}