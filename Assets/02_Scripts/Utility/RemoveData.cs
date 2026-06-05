using UnityEditor;
using UnityEngine;

public class RemoveData : MonoBehaviour
{
    public void Reset()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs has been reset.");
    }
}
