using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace GameAnalyticsSDK
{
	//Inspired from answer on answers.unity3d.com: http://answers.unity3d.com/questions/221651/yielding-with-www-in-editor.html
	public static class GA_ContinuationManager
	{
		private class EditorCoroutine
		{
			public EditorCoroutine(IEnumerator routine, Func<bool> done)
			{
				Routine = routine;
				Done = done;
			}
			public IEnumerator Routine { get; private set; }
			public Func<bool> Done {get; private set;}
			public Action ContinueWith { get; private set; }
		}

		private static readonly List<EditorCoroutine> jobs = new List<EditorCoroutine>();

		public static void StartCoroutine(IEnumerator routine,Func<bool> done)
		{
			if (!jobs.Any())
			{
				EditorApplication.update += Update;
			}
			jobs.Add(new EditorCoroutine(routine,done));
		}

		private static void Update()
		{
			for (int i = jobs.Count-1; i>=0; --i)
			{
				var jobIt = jobs[i];
#if UNITY_2017_1_OR_NEWER
                if (!jobIt.Routine.MoveNext()) //movenext is false if coroutine completed
                {
                    jobs.RemoveAt(i);
                }
#else
                if (jobIt.Done())
				{
					if (!jobIt.Routine.MoveNext()) //movenext is false if coroutine completed
					{
                        Debug.Log("GA_ContinuationManager.Update: Routine finished");
                        jobs.RemoveAt(i);
					}
				}
#endif
            }
			if (!jobs.Any())
			{
				EditorApplication.update -= Update;
			}
		}
	}
}
