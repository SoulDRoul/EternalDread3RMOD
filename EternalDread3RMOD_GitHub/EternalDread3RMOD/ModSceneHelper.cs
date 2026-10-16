using UnityEngine.SceneManagement;

namespace EternalDread3RMOD
{
    public static class ModSceneHelper
    {
        /// <summary>当前是否在教程场景（IntroVillage）。</summary>
        public static bool IsTutorial()
        {
            return SceneManager.GetActiveScene().name == "IntroVillage";
        }
    }
}