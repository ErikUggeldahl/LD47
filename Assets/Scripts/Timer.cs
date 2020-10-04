using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI text = null;

    float elapsedTime = 0f;

    void Update()
    {
        elapsedTime += Time.deltaTime;
        text.text = FormatTextForTime(elapsedTime);
    }

    string FormatTextForTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = (int)time % 60;

        return String.Format("{0:D2}:{1:D2}", minutes, seconds);
    }
}
