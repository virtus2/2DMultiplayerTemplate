using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    
    public string SaveFilePath;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SaveFilePath = Application.persistentDataPath;
        Debug.Log($"saveFilePath: {SaveFilePath}");
    }
}
