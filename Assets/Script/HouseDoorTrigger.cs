using UnityEngine;

public class HouseDoorTrigger : MonoBehaviour
{
    private bool isPlayerNear = false;
    private NetworkManagerFusion networkManager;

    void Start()
    {
        networkManager = NetworkManagerFusion.Instance;
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (networkManager != null)
            {
                Debug.Log("[Door] E 입력 감지: HouseScene으로 이동");
                networkManager.EnterHouseRoom();
            }
            else
            {
                Debug.LogError("[Door] NetworkManagerFusion을 찾을 수 없습니다.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("[Door] 문 근처입니다. E를 누르면 집에 들어갑니다.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            Debug.Log("[Door] 문에서 멀어졌습니다.");
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
        GUI.Label(rect, "Press E to enter", style);
    }
}