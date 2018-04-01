using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using U3D.Threading.Tasks;
using UnityEngine;

namespace U3D.Threading
{
    /// <summary>
    /// Dispatches actions into the main thread.
    /// </summary>
    public class Dispatcher : MonoBehaviour
    {
        static Dispatcher _instance;
        /// <summary>
        /// Gets the singleton instance.
        /// Script gets instantiated into a gameobject in the scene if needed
        /// </summary>
        /// <value>The instance.</value>
        public static Dispatcher instance
        {
            get
            {
                if (!_initialized)
                {
                    UnityEngine.Debug.LogError("You need to call Initialize on the Main thread before using the Dispatcher");
                    throw new InvalidOperationException("You need to call Initialize on the Main thread before using the Dispatcher");
                }
                return _instance;
            }
        }
        static bool _initialized = false;
        public static void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;

            GameObject go = GameObject.Find(typeof(Dispatcher).Name);
            if (go == null)
                go = new GameObject(typeof(Dispatcher).Name);
            _instance = go.GetComponent<Dispatcher>();
            if (_instance == null)
                _instance = go.AddComponent<Dispatcher>();
            DontDestroyOnLoad(_instance.gameObject);
            _instance.gameObject.SendMessage("InitializeInstance", SendMessageOptions.DontRequireReceiver);
        }

        public void Awake()
        {
            Initialize();
        }

        Queue<Action> m_q = new Queue<Action>();
        void Update()
        {
            if (m_q.Count > 0)
            {
                Action a = m_q.Dequeue();
                if (a != null) // ???
                    a();
            }
        }

        /// <summary>
        /// Execute the Action in the main thread as soon as posible.
        /// </summary>
        /// <param name="a">Action to be executed.</param>
        public void ToMainThread(Action a)
        {
            m_q.Enqueue(a);
        }

        /// <summary>
        /// Executes the Action in the main thread as soon as posible
        /// and returns a Task which monitors its execution.
        /// </summary>
        /// <returns>Task monintoring the execution of the action.</returns>
        /// <param name="a">Action to be executed.</param>
        public Task TaskToMainThread(Action a)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            m_q.Enqueue(() =>
            {
                a();
                tcs.SetResult(true);
            });
            return tcs.Task;
        }
        public Task<TResult> TaskToMainThread<TResult>(Func<TResult> f)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            m_q.Enqueue(() =>
            {
                tcs.SetResult(f());
            });
            return tcs.Task;
        }

        /// <summary>
        /// Execute the Action in the main thread after a delay.
        /// </summary>
        /// <param name="seconds">Execution delay, in seconds.</param>
        /// <param name="a">Action to be executed.</param>
        public void ToMainThreadAfterDelay(System.Double seconds, Action a)
        {
            instance.ToMainThread(() =>
            {
                instance.LaunchCoroutine(instance.ExecuteDelayed(seconds, a));
            });
        }
        System.Collections.IEnumerator ExecuteDelayed(System.Double seconds, Action a)
        {
            yield return new WaitForSeconds((float)seconds);
            m_q.Enqueue(a);
        }

        /// <summary>
        /// Executes the coroutine passed as parameter in the main thread.
        /// </summary>
        /// <param name="firstIterationResult">Coroutine to be executed.</param>
        public void LaunchCoroutine(IEnumerator firstIterationResult)
        {
            instance.StartCoroutine(firstIterationResult);
        }
    }

}