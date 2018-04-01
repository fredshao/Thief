using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DebugPanel : EditorWindow {

    public static bool _debugMode = false;

    public static bool debugMode {
        get { return  _debugMode; }
        set {
            _debugMode = value;
            PlayerPrefs.SetInt("DebugMode", _debugMode ? 1 : 0);
        }
    }

    [MenuItem("Violet/DebugPanel")]
    private static void Init() {
        DebugPanel panel = (DebugPanel)EditorWindow.GetWindow(typeof(DebugPanel));
        panel.Show();
    }

    private void OnGUI() {
        debugMode = EditorGUILayout.Toggle("TestMode", debugMode);
    }

}
