using UnityEngine;
using System.Collections;

public class ReporterMessageReceiver : MonoBehaviour
{
    Reporter _reporter;

    void Start()
    {
        _reporter = gameObject.GetComponent<Reporter>();
    }

    void OnPreStart()
    {
        //To Do : this method is called before initializing reporter, 
        //we can for example check the resultion of our device ,then change the size of reporter
        if (_reporter == null)
            _reporter = gameObject.GetComponent<Reporter>();

        if (Screen.width < 1000)
            _reporter.size = new Vector2(32, 32);
        else
            _reporter.size = new Vector2(48, 48);

        _reporter.UserData = "Put user date here like his account to know which user is playing on this device";
    }

    void OnHideReporter()
    {
        //TO DO : resume your game
    }

    void OnShowReporter()
    {
        //TO DO : pause your game and disable its GUI
    }

    void OnLog(Reporter.Log log)
    {
        //TO DO : put you custom code 
    }
}