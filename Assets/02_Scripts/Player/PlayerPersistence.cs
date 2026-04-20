using UnityEngine;

// Player 오브젝트에 부착
public class PlayerPersistence : MonoBehaviour
{
    private static PlayerPersistence _instance { get; set; } 

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}