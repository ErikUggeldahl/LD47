using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardCap : MonoBehaviour
{
    public enum State
    {
        Idle,
        Alarm,
        Dead,
    }

    const float TRANSITION_TIME = 0.5f;

    Material material;
    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    public void SetState(State state)
    {
        StartCoroutine(TransitionToState(state));
    }

    IEnumerator TransitionToState(State state)
    {
        Color newColor = Color.black;
        switch (state)
        {
            case State.Idle: newColor = Color.green; break;
            case State.Alarm: newColor = Color.red; break;
            case State.Dead: newColor = Color.black; break;
        }

        Color currentColor = material.GetColor("_EmissionColor");
        float progress = 0f;
        while (progress < 1f)
        {
            progress += TRANSITION_TIME * Time.deltaTime;
            material.SetColor("_EmissionColor", Color.Lerp(currentColor, newColor, progress));
            yield return null;
        }
    }
}
