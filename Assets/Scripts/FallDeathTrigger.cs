using UnityEngine;

public class FallDeathTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<CharacterControl>().Die(CharacterControl.DeathSource.Fall);
        }
    }
}
