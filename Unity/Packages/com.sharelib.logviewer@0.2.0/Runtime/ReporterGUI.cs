using UnityEngine;
using System.Collections;

public class ReporterGUI : MonoBehaviour
{
    Reporter _reporter;

    void Awake()
    {
        _reporter = gameObject.GetComponent<Reporter>();
    }

    void OnGUI()
    {
        _reporter.OnGUIDraw();
    }
}