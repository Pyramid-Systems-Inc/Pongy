using UnityEngine;
using UnityEngine.SceneManagement;

namespace PongQuest.Managers
{
    /// <summary>
    /// Add this to your first gameplay scene.
    /// It ensures the Persistent Managers scene is loaded.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // Check if managers are already loaded
            if (GameManager.Instance == null)
            {
                // Load the persistent managers scene additively
                SceneManager.LoadScene("_PersistentManagers", LoadSceneMode.Additive);
            }
        }
    }
}