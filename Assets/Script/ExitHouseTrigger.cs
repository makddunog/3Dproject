using UnityEngine;

public class ExitHouseTrigger : MonoBehaviour
{
    private bool isPlayerNear = false;

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[Exit] E 입력 감지: SampleScene으로 이동");

            if (NetworkManagerFusion.Instance != null)
            {
                NetworkManagerFusion.Instance.EnterVillageRoom();
            }
            else
            {
                Debug.LogError("[Exit] NetworkManagerFusion.Instance를 찾을 수 없습니다.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("[Exit] 문 근처입니다. E를 누르면 마을로 나갑니다.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            Debug.Log("[Exit] 문에서 멀어졌습니다.");
        }
    }

    void OnGUI()
    {
        if (!isPlayerNear)
            return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 32;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        Rect rect = new Rect(0, Screen.height * 0.75f, Screen.width, 50);
        GUI.Label(rect, "Press E to exit", style);
    }
}