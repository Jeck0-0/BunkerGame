using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugShortcuts : MonoBehaviour
{
    float timeScaleBeforePausing;
    
    private void Update()
    {
        //toggle fullscreen
        if (Input.GetKeyDown(KeyCode.F11))
        {
            bool toggle = !Screen.fullScreen;
            Screen.fullScreen = toggle;
        }

        if (Input.GetKeyDown(KeyCode.F1) && SceneManager.sceneCount > 0)
            SceneManager.LoadScene(0);
        if (Input.GetKeyDown(KeyCode.F2) && SceneManager.sceneCount > 1)
            SceneManager.LoadScene(1);
        if (Input.GetKeyDown(KeyCode.F3) && SceneManager.sceneCount > 2)
            SceneManager.LoadScene(2);
        if (Input.GetKeyDown(KeyCode.F4) && SceneManager.sceneCount > 3)
            SceneManager.LoadScene(3);
        if (Input.GetKeyDown(KeyCode.F5) && SceneManager.sceneCount > 4)
            SceneManager.LoadScene(4);
        if (Input.GetKeyDown(KeyCode.F6) && SceneManager.sceneCount > 5)
            SceneManager.LoadScene(5);
        if (Input.GetKeyDown(KeyCode.F7) && SceneManager.sceneCount > 6)
            SceneManager.LoadScene(6);
        if (Input.GetKeyDown(KeyCode.F8) && SceneManager.sceneCount > 7)
            SceneManager.LoadScene(7);

        if(Input.GetKey(KeyCode.LeftControl))
        {
            
            //lower volume
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                AudioListener.volume = Mathf.Clamp01(AudioListener.volume - .1f);
                Debug.Log("Volume " + AudioListener.volume);
            }

            //increase volume
            if (Input.GetKeyDown(KeyCode.Period))
            {
                AudioListener.volume = Mathf.Clamp01(AudioListener.volume + .1f);
                Debug.Log("Volume " + AudioListener.volume);
            }
            

            //Load main scene
            if (Input.GetKeyDown(KeyCode.E))
            {
                SceneManager.LoadScene(0);
                Debug.Log("Loaded main scene");
            }

            //Reload scene
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                Debug.Log("Reloaded scene");
            }

            
            //Speed up
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Time.timeScale *= 2;
                Debug.Log($"TimeScale: {Time.timeScale}");
            }
            
            //Slow down
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Time.timeScale /= 2;
                Debug.Log($"TimeScale: {Time.timeScale}");
            }

            //Reset time scale
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Time.timeScale = 1;
                Debug.Log($"TimeScale: {Time.timeScale}");
            }

            //Toggle pause
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (Time.timeScale > 0f)
                {
                    timeScaleBeforePausing = Time.timeScale;
                    Time.timeScale = 0;
                    Debug.Log("Paused");
                }
                else
                {
                    Time.timeScale = timeScaleBeforePausing;
                    Debug.Log("Resumed");
                }
            }
        }
    }
}
