using UnityEngine;

public class TrainingTarget : MonoBehaviour
{
    [SerializeField] new AudioSource audio = null;

    Color originalColor;
    void Start()
    {
        var currentColor = GetComponent<Renderer>().material.color;
        originalColor = new Color(currentColor.r, currentColor.g, currentColor.b);
    }

    public void Approve()
    {
        GetComponent<Renderer>().material.color = Color.green;

        audio.Play();
    }

    public void Deny()
    {
        GetComponent<Renderer>().material.color = originalColor;
    }
}
