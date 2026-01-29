using UnityEngine;

public class mainMenu_Buttons : MonoBehaviour
{
    public void actionExit()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
