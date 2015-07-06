using SampleModels;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using I = IteratorTasks;

public class SampleBehaviour : MonoBehaviour
{
    [SerializeField]
    Text _textOutput;

    void Start()
    {
        var s = new Subject<string>();

        var scheduler = SchedulerUnity.MainThread;

        s.ObserveOn(scheduler).Subscribe(
            result =>
            {
                Debug.Log(result);
                _textOutput.text += result.Substring(0, Math.Min(result.Length, 100)) + "\n";
            },
            ex =>
            {
                Debug.Log(ex);
                _textOutput.text += ex.Message + "\n";
            }
            );

        Class1.MainAsync(s);
    }

    void Update()
    {
        var s = I.Task.DefaultScheduler;
        s.Update();
    }
}
