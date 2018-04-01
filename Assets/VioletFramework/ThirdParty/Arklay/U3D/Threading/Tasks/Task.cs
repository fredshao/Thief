using System;
using System.Collections;
using System.Collections.Generic;

namespace U3D.Threading.Tasks
{
    class TaskAbortException : Exception
    {

    }

    public class Task
    {
        public static void Delay(int milliseconds)
        {
#if UNITY_WSA && !UNITY_EDITOR
            System.Threading.Tasks.Task.Delay(milliseconds).Wait();
#else
            System.Threading.Thread.Sleep(milliseconds);
#endif
        }

        protected enum TState { Created, Running, Successful, Aborted, Faulted };
        TState __state = TState.Created;

        List<Action<Task>> m_continuationActions = new List<Action<Task>>();

        object _lockObject = new object();
        protected TState m_state
        {
            get { return __state; }
            set
            {
                if (value != __state)
                {
                    lock (_lockObject)
                    {
                        switch (value)
                        {
                            case TState.Running:
                            case TState.Created:
                                if (__state != TState.Created)
                                    throw new InvalidOperationException(string.Format("Invalid state transition; {0} -> {1}", __state, value));
                                break;
                            case TState.Successful:
                            case TState.Aborted:
                            case TState.Faulted:
                                if (__state != TState.Running)
                                    throw new InvalidOperationException(string.Format("Invalid state transition; {0} -> {1}", __state, value));
                                __state = value;
                                List<Action<Task>> aux = m_continuationActions;
                                m_continuationActions = new List<Action<Task>>();
                                foreach (Action<Task> a in aux)
                                {
                                    a(this);
                                }
                                break;
                            default:
                                throw new InvalidOperationException(string.Format("Unexpected state; {0}", value));
                        }
                        __state = value;
                    }
                    if (m_state == TState.Successful || m_state == TState.Aborted || m_state == TState.Faulted)
                    {
                        while (m_whenDone.Count > 0)
                        {
                            lock (m_whenDoneSync)
                            {
                                m_whenDone.Dequeue().Invoke(this);
                            }
                        }
                    }
                }
            }
        }

        protected Action m_action;

        protected Task()
        {
            U3D.Threading.Dispatcher.Initialize(); // just in case...
        }

        protected Task(Action action) : this()
        {
            m_action = action;
        }

        /// <summary>
        /// Gets the <see cref="T:System.AggregateException">Exception</see> that caused the <see
        /// cref="Task">Task</see> to end prematurely. If the <see
        /// cref="Task">Task</see> completed successfully or has not yet thrown any
        /// exceptions, this will return null.
        /// </summary>
        /// <remarks>
        /// Tasks that throw unhandled exceptions store the resulting exception and propagate it wrapped in a
        /// <see cref="System.AggregateException"/> in calls to <see cref="Wait()">Wait</see>
        /// or in accesses to the <see cref="Exception"/> property.  Any exceptions not observed by the time
        /// the Task instance is garbage collected will be propagated on the finalizer thread.
        /// </remarks>
        public AggregateException Exception
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets whether this <see cref="Task">Task</see> has completed.
        /// </summary>
        /// <remarks>
        /// <see cref="IsCompleted"/> will return true when the Task is in one of the three
        /// final states: <see cref="System.Threading.Tasks.TaskStatus.RanToCompletion">RanToCompletion</see>,
        /// <see cref="System.Threading.Tasks.TaskStatus.Faulted">Faulted</see>, or
        /// <see cref="System.Threading.Tasks.TaskStatus.Canceled">Canceled</see>.
        /// </remarks>
        public bool IsCompleted
        {
            get
            {
                return m_state == TState.Successful || m_state == TState.Aborted || m_state == TState.Faulted;
            }
        }

        /// <summary>
        /// Gets whether the <see cref="Task"/> completed due to an unhandled exception.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsFaulted"/> is true, the Task's <see cref="Status"/> will be equal to
        /// <see cref="System.Threading.Tasks.TaskStatus.Faulted">TaskStatus.Faulted</see>, and its
        /// <see cref="Exception"/> property will be non-null.
        /// </remarks>
        public bool IsFaulted
        {
            get
            {
                return m_state == TState.Faulted;
            }
        }

        // workaround to mock the threads abort (not implemented in unity's version of .net
        public void CheckAbort()
        {
            if (m_runThread.aborted)
                throw new TaskAbortException();
        }
        public bool IsAborted
        {
            get
            {
                return m_state == TState.Aborted;
            }
        }

        #region default thread pool methods
#if !UNITY_WEBGL
        static ThreadPool __defaultThreadPool;
        /// <summary>
        /// Gets or sets the default thread pool for the tasks. 
        /// (It will take effect for tasks created after the call)
        /// (It creates a default thread pool with a maximum of 100 threads)
        /// </summary>
        /// <value>The default thread pool.</value>
        public static ThreadPool defaultThreadPool
        {
            get
            {
                if (__defaultThreadPool == null)
                {
                    __defaultThreadPool = new ThreadPool();
                    __defaultThreadPool.maxThreads = 100;
                }
                return __defaultThreadPool;
            }
            set
            {
                __defaultThreadPool = value;
            }
        }
#endif
        #endregion

        #region Run methods
        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task handle for that work.
        /// </summary>
        /// <param name="action">The work to execute asynchronously</param>
        /// <returns>A Task that represents the work queued to execute in the ThreadPool.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> parameter was null.
        /// </exception>
#if UNITY_WEBGL
        public static Task Run(Action action)
        {
            return RunInMainThread(action);
        }
#else
        public static Task Run(Action action, ThreadPool tp = null)
        {
            Task t = new Task(action);
            t.RunAsync((tp == null) ? defaultThreadPool : tp);
            return t;
        }
#endif

        /// <summary>
        /// Queues the specified work to run on the ThreadPool and returns a Task handle for that work, the action is guaranteed to be executed in the main thread
        /// </summary>
        /// <param name="action">The work to execute in the main thread</param>
        /// <returns>A Task that represents the work queued to execute in the ThreadPool.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="action"/> parameter was null.
        /// </exception>
        public static Task RunInMainThread(Action action)
        {
            Dispatcher.Initialize();
            return Dispatcher.instance.TaskToMainThread(action);
        }

#if UNITY_WEBGL
        protected Task RunAsync()
#else
        ThreadPool.PooledThread m_runThread;
        protected Task RunAsync(ThreadPool threadPool)
#endif
        {
            /*
             * The coroutine approach doesnt work:
             * Unity executes the coroutine in the update cycle (main thread) so, 
             * if there is a Wait in the Task (most likely) the main thread will get frozen
             */
            //Dispatcher.instance.LaunchCoroutine(RunCoroutine());
            Exception = null;
            m_state = TState.Running;
#if UNITY_WEBGL
            U3D.Threading.Dispatcher.instance.ToMainThread(() =>
#else
            m_runThread = threadPool.QueueAction(() =>
#endif
            {
                try
                {
                    m_action();
                    CheckAbort();
                    m_state = TState.Successful;
                }
                catch (TaskAbortException)
                {
                    m_state = TState.Aborted;
                }
#if !UNITY_WSA && !UNITY_EDITOR
                catch (System.Threading.ThreadAbortException)
                {
                    m_state = TState.Aborted;
                }
#endif
                catch (Exception e)
                {
                    this.Exception = new AggregateException(e);
                    m_state = TState.Faulted;
                }
                finally
                {
#if !UNITY_WEBGL
                    m_runThread = null;
#endif
                }
            });
            return this;
        }

        public virtual void AbortThread()
        {
#if !UNITY_WEBGL
            m_runThread.Abort();
#endif
        }
        #endregion

        /// <summary>
        /// Waits for the <see cref="Task"/> to complete execution.
        /// </summary>
        /// <exception cref="T:System.AggregateException">
        /// The <see cref="Task"/> was canceled -or- an exception was thrown during
        /// the execution of the <see cref="Task"/>.
        /// </exception>
        public void Wait(Action<Task> whenFinished)
        {
            if (!IsCompleted)
            {
                m_continuationActions.Add(whenFinished);
            }
            else
            {
                whenFinished(this);
            }
        }

        #region Continuation methods
        /// <summary>
        /// Creates a continuation that executes when the target <see cref="Task"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task"/> completes. When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        public Task ContinueWith(Action<Task> continuationAction)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            this.Wait((t) => {
                try
                {
                    continuationAction(t);
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetError(e);
                }
            });
            return tcs.Task;
        }
        /// <summary>
        /// Creates a continuation that executes on the Main thread when the target <see cref="Task"/> completes.
        /// </summary>
        /// <param name="continuationAction">
        /// An action to run when the <see cref="Task"/> completes. When run, the delegate will be
        /// passed the completed task as an argument.
        /// </param>
        /// <returns>A new continuation <see cref="Task"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Task"/> will not be scheduled for execution until the current task has
        /// completed, whether it completes due to running to completion successfully, faulting due to an
        /// unhandled exception, or exiting out early due to being canceled.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="continuationAction"/> argument is null.
        /// </exception>
        public Task ContinueInMainThreadWith(Action<Task> continuationAction)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            this.Wait((t) => {
                Dispatcher.instance.ToMainThread(() =>
                {
                    try
                    {
                        continuationAction(t);
                        tcs.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        tcs.SetError(e);
                    }
                });
            });
            return tcs.Task;
        }

        Queue<Action<Task>> m_whenDone = new Queue<Action<Task>>();
        object m_whenDoneSync = new object();
        internal void AddCompletionAction(Action<Task> continuation)
        {
            lock (m_whenDoneSync)
            {
                m_whenDone.Enqueue(continuation);
            }
        }
        #endregion

        #region WhenAll
        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        /// <remarks>
        /// <para>
        /// If any of the supplied tasks completes in a faulted state, the returned task will also complete in a Faulted state, 
        /// where its exceptions will contain the aggregation of the set of unwrapped exceptions from each of the supplied tasks.  
        /// </para>
        /// <para>
        /// If none of the supplied tasks faulted but at least one of them was canceled, the returned task will end in the Canceled state.
        /// </para>
        /// <para>
        /// If none of the tasks faulted and none of the tasks were canceled, the resulting task will end in the RanToCompletion state.   
        /// </para>
        /// <para>
        /// If the supplied array/enumerable contains no tasks, the returned task will immediately transition to a RanToCompletion 
        /// state before it's returned to the caller.  
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="tasks"/> argument was null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <paramref name="tasks"/> array contained a null task.
        /// </exception>
        public static Task WhenAll(params Task[] tasks)
        {
            // Do some argument checking and make a defensive copy of the tasks array
            if (tasks == null) throw new ArgumentNullException("Tasks cannot be null");

            int taskCount = tasks.Length;
            if (taskCount == 0) return InternalWhenAll(tasks); // Small optimization in the case of an empty array.

            Task[] tasksCopy = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                Task task = tasks[i];
                if (task == null) throw new ArgumentException("A task cannot be null");
                tasksCopy[i] = task;
            }

            // The rest can be delegated to InternalWhenAll()
            return InternalWhenAll(tasksCopy);
        }
        public static Task WhenAll(List<Task> tasks)
        {
            return WhenAll(tasks.ToArray());
        }

        // Some common logic to support WhenAll() methods
        // tasks should be a defensive copy.
        private static Task InternalWhenAll(Task[] tasks)
        {
            return (tasks.Length == 0) ? // take shortcut if there are no tasks upon which to wait
                Task.CompletedTask :
                new WhenAllPromise(tasks);
        }

        /// <summary>A task that's already been completed successfully.</summary>
        private static Task s_completedTask;
        internal static Task CompletedTask
        {
            get
            {
                if (s_completedTask == null)
                {
                    s_completedTask = new Task(() => { });
#if UNITY_WEBGL
                    s_completedTask.RunAsync();
#else
                    s_completedTask.RunAsync(defaultThreadPool);
#endif
                }
                return s_completedTask;
            }
        }

        private sealed class WhenAllPromise : Task
        {
            private Task[] m_tasks;

            public WhenAllPromise(Task[] tasks)
            {
                this.m_tasks = tasks;
                foreach (var task in tasks)
                {
                    if (task.IsCompleted) this.Invoke(task); // short-circuit the completion action, if possible
                    else task.AddCompletionAction(Invoke); // simple completion action
                }
            }

            public override void AbortThread()
            {
                foreach (Task i in m_tasks)
                {
                    if (i.m_state == TState.Running)
                        i.AbortThread();
                }
            }

            private void Invoke(Task task)
            {
                if (m_state == TState.Created)
                    m_state = TState.Running;
                TState s = TState.Successful;
                AggregateException ex = null;
                foreach (Task i in m_tasks)
                {
                    switch (i.m_state)
                    {
                        case TState.Created:
                        case TState.Running:
                            s = TState.Running;
                            break;
                        case TState.Successful:
                        case TState.Aborted:
                            break;
                        case TState.Faulted:
                            if (ex == null) ex = new AggregateException();
                            ex.AddException(i.Exception);
                            break;
                        default:
                            throw new InvalidOperationException("Unknown task state: " + i.m_state);
                    }
                }
                if (ex != null && s == TState.Successful)
                    s = TState.Faulted;
                this.Exception = ex;
                m_state = s;
            }

        }
        #endregion
        #region WhenAny
        public static Task WhenAny(params Task[] tasks)
        {
            // Do some argument checking and make a defensive copy of the tasks array
            if (tasks == null) throw new ArgumentNullException("Tasks cannot be null");

            int taskCount = tasks.Length;
            if (taskCount == 0) return InternalWhenAny(tasks); // Small optimization in the case of an empty array.

            Task[] tasksCopy = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                Task task = tasks[i];
                if (task == null) throw new ArgumentException("A task cannot be null");
                tasksCopy[i] = task;
            }

            // The rest can be delegated to InternalWhenAny()
            return InternalWhenAny(tasksCopy);
        }
        public static Task WhenAny(List<Task> tasks)
        {
            return WhenAny(tasks.ToArray());
        }

        // Some common logic to support WhenAny() methods
        // tasks should be a defensive copy.
        private static Task InternalWhenAny(Task[] tasks)
        {
            return (tasks.Length == 0) ? // take shortcut if there are no tasks upon which to wait
                Task.CompletedTask :
                    new WhenAnyPromise(tasks);
        }

        private sealed class WhenAnyPromise : Task
        {
            private Task[] m_tasks;

            public WhenAnyPromise(Task[] tasks)
            {
                this.m_tasks = tasks;
                foreach (var task in tasks)
                {
                    if (task.IsCompleted)
                    {
                        this.Invoke(task); // short-circuit the completion action, if possible
                        return;
                    }
                    else
                        task.AddCompletionAction(Invoke); // simple completion action
                }
            }

            public override void AbortThread()
            {
                foreach (Task i in m_tasks)
                {
                    if (i.m_state == TState.Running)
                        i.AbortThread();
                }
            }

            private void Invoke(Task task)
            {
                if (m_state == TState.Created)
                    m_state = TState.Running;
                TState s = TState.Running;

                AggregateException ex = null;
                foreach (Task i in m_tasks)
                {
                    switch (i.m_state)
                    {
                        case TState.Created:
                        case TState.Running:
                            break;
                        case TState.Successful:
                        case TState.Aborted:
                            s = TState.Successful;
                            break;
                        case TState.Faulted:
                            if (ex == null) ex = new AggregateException();
                            ex.AddException(i.Exception);
                            break;
                        default:
                            throw new InvalidOperationException("Unknown task state: " + i.m_state);
                    }
                }
                if (ex != null && s == TState.Successful)
                    s = TState.Faulted;
                this.Exception = ex;
                m_state = s;
            }

        }
        #endregion
    }
}