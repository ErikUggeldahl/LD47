using System.Collections;
using UnityEngine;

public class Revealer : MonoBehaviour
{
    [SerializeField] Material revealMaterial = null;

    const float REVEALED_ALPHA = 0.3f;
    const float FADE_TIME = 1f;

    Material originalMaterial;

    Material solidMaterial;
    Material revealedMaterial;

    void Start()
    {
        tag = "Occluder";

        originalMaterial = GetComponent<Renderer>().sharedMaterial;

        solidMaterial = new Material(revealMaterial);

        revealedMaterial = new Material(revealMaterial);
        Color revealedColor = revealedMaterial.color;
        revealedColor.a = REVEALED_ALPHA;
        revealedMaterial.color = revealedColor;
    }

    public void Reveal()
    {
        StopAllCoroutines();
        StartCoroutine(RevealCoroutine());
    }

    IEnumerator RevealCoroutine()
    {
        Material material = new Material(solidMaterial);
        GetComponent<Renderer>().material = material;

        float progress = 0f;
        while (progress < 1f)
        {
            progress += FADE_TIME * Time.deltaTime;

            material.Lerp(solidMaterial, revealedMaterial, progress);

            yield return null;
        }
    }

    public void Solidify()
    {
        StopAllCoroutines();
        StartCoroutine(SolidifyCoroutine());
    }

    IEnumerator SolidifyCoroutine()
    {
        Material material = GetComponent<Renderer>().material;

        float progress = 0f;
        while (progress < 1f)
        {
            progress += FADE_TIME * Time.deltaTime;

            material.Lerp(revealedMaterial, solidMaterial, progress);

            yield return null;
        }

        GetComponent<Renderer>().material = originalMaterial;
    }
}
