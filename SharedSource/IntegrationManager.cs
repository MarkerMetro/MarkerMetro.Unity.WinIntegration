using System;
using UnityEngine;

namespace MarkerMetro.Unity.WinIntegration
{

//// Problem happening on WP8.
// Because of a bug on Unity this class won't work on WP8.
// More details here: http://fogbugz.unity3d.com/default.asp?609123_4r6pfprov9jibi2g
// For this reason, this class was stubbed on WP8, please remove the stub code after the bug gets fixed.

#if NETFX_CORE 
    /**
     * This singleton is an entry point for Marker Metro MonoBehaviours.
     * It creates a Game Object that is not destroyed between scene loads.
     * It offers an easy way to hook up scripts from the game code or from the UI thread and
     * contains Windows-related events.
     */
    public sealed class IntegrationManager : MonoBehaviour
    {
        private static IntegrationManager instance = null;
        public static IntegrationManager Instance
        {
            get
            {
                if (instance == null) Init();
                return instance;
            }
        }

        /**
         * Fire every time the Windows key is down.
         */
        public event Action OnWindowsKeyDown;
        
        /**
         * Fired every MonoBehaviour Update.
         */
        public event Action OnUpdate;
        
        /**
         * Creates a Game Object and adds the IntegrationManager in it.
         */
        public static void Init()
        {
            if (instance != null) return;

            if (!IsWinRT())
                throw new PlatformNotSupportedException();

            GameObject go = new GameObject("WindowsIntegrationManager");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<IntegrationManager>();
        }

        /**
         * Instantiates a MonoBehaviour of the given type inside the Manager's Game Object.
         */
        public void AddComponent<T>() where T : MonoBehaviour
        {
            gameObject.AddComponent<T>();
        }

        /**
         * Destroy the singleton and it's GameObject.
         */
        public void Destroy()
        {
            Destroy(gameObject);
            instance = null;
        }

        private static bool IsWinRT()
        {
            return 
                Application.platform == RuntimePlatform.MetroPlayerARM ||
                Application.platform == RuntimePlatform.MetroPlayerX64 ||
                Application.platform == RuntimePlatform.MetroPlayerX86 ||
                Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WP8Player;
        }

        private void Update()
        {
            if (OnUpdate != null)
                OnUpdate();

            if ((Input.GetKeyDown(KeyCode.LeftWindows) ||
                Input.GetKeyDown(KeyCode.RightWindows) ||
                Input.GetKeyDown(KeyCode.LeftApple) ||
                Input.GetKeyDown(KeyCode.RightApple))
                && OnWindowsKeyDown != null)
                OnWindowsKeyDown();
        }
    }
#else
    public sealed class IntegrationManager
    {
        private static IntegrationManager instance = null;
        public static IntegrationManager Instance
        {
            get
            {
                if (instance == null) Init();
                return instance;
            }
        }
        public event Action OnWindowsKeyDown;
        public event Action OnUpdate;
        public static void Init()
        {
            if (instance != null) return;
        }
        public void AddComponent<T>()
        {
        }
        public void Destroy()
        {
            instance = null;
        }
    }
#endif
}
