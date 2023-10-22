using UnityEngine;
using System.IO;

public class WriteDebugToFile : MonoBehaviour
{
    [SerializeField] bool WriteToFile = false;
    string fileName = "";

    void OnEnable() {
        Application.logMessageReceived += Log;
    }

    void OnDisable() {
        Application.logMessageReceived -= Log;
    }

    void Start() {
        if(WriteToFile) {
            fileName = Application.dataPath + "/LogFile.text";
            TextWriter tw = new StreamWriter(fileName, false);
            tw.Close();
        }
    }

    public void Log(string logString, string stackTrace, LogType type) {
        if(WriteToFile) {
            TextWriter tw = new StreamWriter(fileName, true);
            tw.WriteLine(logString);
            tw.Close();
        }
    }
}
