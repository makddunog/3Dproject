using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Voice.Unity;

public class VoiceStatusUI : MonoBehaviour
{
    public Recorder recorder;
    public TMP_Text statusText;
    public Image micIcon;

    void Update()
    {
        if (recorder == null || statusText == null)
            return;

        if (recorder.IsCurrentlyTransmitting)
        {
            statusText.text = "MIC ON";
            if (micIcon != null)
                micIcon.enabled = true;
        }
        else
        {
            statusText.text = "MIC OFF";
            if (micIcon != null)
                micIcon.enabled = false;
        }
    }
}