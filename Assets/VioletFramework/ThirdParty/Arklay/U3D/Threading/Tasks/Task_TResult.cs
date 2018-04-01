using System;
using System.Collections;
using System.Collections.Generic;

namespace U3D.Threading.Tasks
{
    public class Task<TResult> : Task
    {
        public TResult Result { get; private set; }
		
		// internal helper function breaks out logic used by TaskCompletionSource
		public Task()
			: base()
		{
			Result = default(TResult);
		}
		
		public bool TrySetResult(TResult result)
		{
			if (IsCompleted) return false;
			
			if (m_state == TState.Created)
				m_state = TState.Running;
			Result = result;
			m_state = TState.Successful;
			return true;
		}
		public bool TrySetError(Exception e)
		{
			if (IsCompleted) return false;
			
			if(m_state== TState.Created)
				m_state = TState.Running;
			Exception = new AggregateException(e);
			m_state = TState.Faulted;
			return true;
		}
		// end public functions for TaskCompletionSource

		protected Task(Func<TResult> f)
            : base()
        {
            Result = default(TResult);
			m_action= () => {
				Result = f();
			};
        }

#if UNITY_WEBGL
        public static Task<TResult> Run(Func<TResult> action)
        {
            Task<TResult> t = new Task<TResult>(action);
            t.RunAsync();
            return t;
        }
#else
        public static Task<TResult> Run(Func<TResult> action, ThreadPool tp= null)
        {
            Task<TResult> t = new Task<TResult>(action);
			t.RunAsync ((tp == null) ? defaultThreadPool : tp);
			return t;
		}
#endif
        public static Task<TResult> RunInMainThread(Func<TResult> action)
        {
            Dispatcher.Initialize();
            return Dispatcher.instance.TaskToMainThread(action);
        }

		public void Wait(Action<Task<TResult>> whenFinished)
		{
			base.Wait ((t) => {
				whenFinished((Task<TResult>)t);
			});
		}
        public Task ContinueWith(Action<Task<TResult>> continuationAction)
        {
			return base.ContinueWith ((t) => {
				continuationAction((Task<TResult>)t);
			});
        }
        public Task ContinueInMainThreadWith(Action<Task<TResult>> continuationAction)
        {
			return base.ContinueInMainThreadWith ((t) => {
				continuationAction((Task<TResult>)t);
			});
        }

        internal void SetIsRunning()
        {
            m_state = TState.Running;
        }
    }
}