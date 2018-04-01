using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using U3D.Threading.Tasks;
using UnityEngine;
#if !UNITY_WEBGL

namespace U3D.Threading
{
    public class Thread
    {
#if UNITY_WSA && !UNITY_EDITOR
        System.Threading.Tasks.Task m_impl;
        public static implicit operator System.Threading.Tasks.Task(Thread t)
        {
            return t.m_impl;
        }
        public static implicit operator Thread(System.Threading.Tasks.Task t)
        {
            return new Thread() { m_impl = t };
        }

        public void Start(Thread th)
        {
            // is already started m_impl.Start(th);
        }
#else
        System.Threading.Thread m_impl;
        public static implicit operator System.Threading.Thread(Thread t)
        {
            return t.m_impl;
        }
        public static implicit operator Thread(System.Threading.Thread t)
        {
            return new Thread() { m_impl = t };
        }

        public void Start(Thread th)
        {
            m_impl.Start(th);
        }
#endif
    }

    /// <summary>
    /// Provides a pool of threads that can be used to execute tasks, post work items, process 
    /// asynchronous I/O, wait on behalf of other threads, and process timers.
    /// </summary>
    public class ThreadPool
	{
		/// <summary>
		/// The number of requests to the thread pool that can be active concurrently. 
		/// All requests above that number remain queued until thread pool threads become available.
		/// </summary>
		public int maxThreads = 1;
		/// <summary>
		/// The difference between the maximum number of thread pool threads returned by the 
		/// GetMaxThreads method, and the number currently active.
		/// </summary>
		public int availableThreads
		{
			get { return maxThreads - m_currentThreads.Count; }
		}

		/// <summary>
		/// Describes a pooled thread
		/// </summary>
		public class PooledThread
		{
			internal Thread thread;
			internal bool aborted = false;
			internal Action action;
			/// <summary>
			/// Gets a value indicating whether this <see cref="U3D.Threading.ThreadPool+PooledThread"/> is enqueued.
			/// </summary>
			/// <value><c>true</c> if is enqueued; otherwise, <c>false</c>.</value>
			public bool isEnqueued
			{
				get { return thread == null; }
			}
			/// <summary>
			/// Aborts the pooled thread
			/// (The thread may not be executing, and it won't receive the ThreadAborted exception)
			/// </summary>
			public void Abort()
			{
                // abort is not implemented by unity's version of .net, it will NEVER work
                //if (thread != null) 
				//{
				//	thread.Abort (); 
				//} 
				aborted = true;
			}
		}

		object m_sync = new object ();
		List<Thread> m_currentThreads = new List<Thread>();
		Queue<PooledThread> m_queuedActions = new Queue<PooledThread> ();
		/// <summary>
		/// Queues a method for execution. The method executes when a thread pool thread becomes available.
		/// </summary>
		/// <returns>PooledThread object describing the thread and its status.</returns>
		/// <param name="a">The action to be executed in the thread.</param>
		public PooledThread QueueAction(Action a)
		{
			PooledThread pt = new PooledThread () { action = a };
			lock (m_sync) 
			{
				if (availableThreads > 0) 
				{
#if UNITY_WSA && !UNITY_EDITOR
                    Thread th = new System.Threading.Tasks.Task(a).ContinueWith((_th) => 
                    {
                        ThreadWillFinish((Thread)_th);
                    });
#else

                    Thread th = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart((_th) =>
				    {
					    try
					    {								
						    a();
					    }
					    finally
					    {
						    ThreadWillFinish((Thread)_th);
					    }
				    }));
#endif
					pt.thread = th;
                    m_currentThreads.Add (th);
					th.Start(th);
				} 
				else 
				{
					m_queuedActions.Enqueue (pt);
				}
			}
			return pt;
		}

		void ThreadWillFinish(Thread th)
		{
			Action a = null;
			lock (m_sync) 
			{
				if (m_queuedActions.Count > 0) 
				{
					PooledThread pt = m_queuedActions.Dequeue ();
					if (pt.aborted) 
					{
						ThreadWillFinish (th);
					} 
					else 
					{
						pt.thread = th;
						a = pt.action;
					}
				}
				else 
				{
					m_currentThreads.Remove (th);
				}
			}
			if (a != null) 
			{
				a ();
				ThreadWillFinish (th);
			} 
		}
	}
}
#endif