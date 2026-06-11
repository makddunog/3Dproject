using UnityEngine;
using TMPro;
using Photon.Voice.Unity;

public class MicToggleUI : MonoBehaviour
{
    public Recorder recorder;
    public TMP_Text statusText;

    private bool micOn = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            micOn = !micOn;

            if (recorder != null)
            {
                recorder.TransmitEnabled = micOn;
                recorder.RecordingEnabled = micOn;
            }
        }

        if (statusText != null)
        {
            statusText.text = micOn ? "MIC ON" : "MIC OFF";
        }
    }
}