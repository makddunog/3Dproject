using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class HouseSceneSpawnFix : NetworkBehaviour
{
    private bool movedInHouse = false;

    void Update()
    {
        if (Object == null)
            return;

        // Shared Mode 기준: 내 캐릭터만 직접 이동
        if (!Object.HasStateAuthority && !Object.HasInputAuthority)
            return;

        if (SceneManager.GetActiveScene().name == "HouseScene" && !movedInHouse)
        {
            MoveToHousePosition();
            movedInHouse = true;
        }

        if (SceneManager.GetActiveScene().name != "HouseScene")
        {
            movedInHouse = false;
        }
    }

    private void MoveToHousePosition()
    {
        CharacterController controller = GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        transform.position = new Vector3(-3.1f, 1.2f, 2.4f);
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        if (controller != null)
        {
            controller.enabled = true;
        }

        Camera[] playerCameras = GetComponentsInChildren<Camera>(true);

        foreach (Camera cam in playerCameras)
        {
            cam.gameObject.SetActive(true);
            cam.enabled = true;
            cam.tag = "MainCamera";
        }

        AudioListener[] listeners = GetComponentsInChildren<AudioListener>(true);

        foreach (AudioListener listener in listeners)
        {
            listener.enabled = true;
        }

        Debug.Log("[HouseSceneSpawnFix] 플레이어를 침대 근처 위치로 강제 이동 완료: " + transform.position);
    }
}