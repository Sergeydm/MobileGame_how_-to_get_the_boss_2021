namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class LoadDemoScene : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}