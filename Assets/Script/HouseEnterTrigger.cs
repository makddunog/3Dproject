using UnityEngine;

public class HouseEnterTrigger : MonoBehaviour
{
    public Transform houseSpawnPoint;
    public GameObject enterText;

    private bool isPlayerNear = false;
    private Transform player;

    void Start()
    {
        if (enterText != null)
            enterText.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            EnterHouse();
        }
    }

    void EnterHouse()
    {
        if (player != null && houseSpawnPoint != null)
        {
            player.position = houseSpawnPoint.position;
            player.rotation = houseSpawnPoint.rotation;
        }

        if (enterText != null)
            enterText.SetActive(false);

        Debug.Log("집에 입장했습니다.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            player = other.transform;

            if (enterText != null)
                enterText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            player = null;

            if (enterText != null)
                enterText.SetActive(false);
        }
    }
}