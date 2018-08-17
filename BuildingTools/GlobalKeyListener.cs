using BrilliantSkies.Core.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingTools
{
    public class GlobalKeyListener : MonoBehaviour
    {
        public static List<KeyPressEvent> Events { get; } = new List<KeyPressEvent>();

        public static GlobalKeyListener Instance { get; }

        static GlobalKeyListener()
        {
            var listenerObject = new GameObject("KeyListener");
            listenerObject.transform.SetParent(null);
            DontDestroyOnLoad(listenerObject);
            Instance = listenerObject.AddComponent<GlobalKeyListener>();
        }

        private void Update()
        {
            foreach (KeyPressEvent ev in Events)
            {
                ev.CheckAndCallEvents();
            }
        }
    }
}
