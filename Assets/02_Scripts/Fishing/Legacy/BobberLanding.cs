using UnityEngine;

public class BobberLanding : MonoBehaviour
{
    public FishingCaster fishingCaster;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water") && fishingCaster.state == FishingCaster.State.Flying)
        {
            fishingCaster.OnBobberLanded();
        }
    }
}