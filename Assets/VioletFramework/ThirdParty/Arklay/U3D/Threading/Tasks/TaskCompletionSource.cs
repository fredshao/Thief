using System;
namespace U3D.Threading.Tasks
{
    public class TaskCompletionSource<TResult>
    {
        private readonly Task<TResult> m_task;
        
        /// <summary>
        /// Creates a <see cref="TaskCompletionSource{TResult}"/>.
        /// </summary>
        public TaskCompletionSource()
        {
            m_task = new Task<TResult>();
        }

        /// <summary>
        /// Gets the <see cref="T:System.Threading.Tasks.Task{TResult}"/> created
        /// by this <see cref="TaskCompletionSource{TResult}"/>.
        /// </summary>
        /// <remarks>
        /// This property enables a consumer access to the <see
        /// cref="T:System.Threading.Tasks.Task{TResult}"/> that is controlled by this instance.
        /// The <see cref="SetResult"/>, <see cref="SetException(System.Exception)"/>,
        /// <see cref="SetException(System.Collections.Generic.IEnumerable{System.Exception})"/>, and <see cref="SetCanceled"/>
        /// methods (and their "Try" variants) on this instance all result in the relevant state
        /// transitions on this underlying Task.
        /// </remarks>
        public Task<TResult> Task
        {
            get { return m_task; }
        }

        /// <summary>Spins until the underlying task is completed.</summary>
        /// <remarks>This should only be called if the task is in the process of being completed by another thread.</remarks>
        private void SpinUntilCompleted()
        {
            // Spin wait until the completion is finalized by another thread.
            //var sw = new SpinWait();
            while (!m_task.IsCompleted)
            {
                Tasks.Task.Delay(10);
            }
        }

        /// <summary>
        /// Attempts to transition the underlying
        /// <see cref="T:System.Threading.Tasks.Task{TResult}"/> into the 
        /// <see cref="System.Threading.Tasks.TaskStatus.RanToCompletion">RanToCompletion</see>
        /// state.
        /// </summary>
        /// <param name="result">The result value to bind to this <see 
        /// cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        /// <remarks>This operation will return false if the 
        /// <see cref="T:System.Threading.Tasks.Task{TResult}"/> is already in one
        /// of the three final states:
        /// <see cref="System.Threading.Tasks.TaskStatus.RanToCompletion">RanToCompletion</see>, 
        /// <see cref="System.Threading.Tasks.TaskStatus.Faulted">Faulted</see>, or
        /// <see cref="System.Threading.Tasks.TaskStatus.Canceled">Canceled</see>.
        /// </remarks>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="Task"/> was disposed.</exception>
        public bool TrySetResult(TResult result)
        {
            return m_task.TrySetResult(result);
        }
        public bool TrySetError(Exception error)
        {
            return m_task.TrySetError(error);
        }

        /// <summary>
        /// Transitions the underlying
        /// <see cref="T:System.Threading.Tasks.Task{TResult}"/> into the 
        /// <see cref="System.Threading.Tasks.TaskStatus.RanToCompletion">RanToCompletion</see>
        /// state.
        /// </summary>
        /// <param name="result">The result value to bind to this <see 
        /// cref="T:System.Threading.Tasks.Task{TResult}"/>.</param>
        /// <exception cref="T:System.InvalidOperationException">
        /// The underlying <see cref="T:System.Threading.Tasks.Task{TResult}"/> is already in one
        /// of the three final states:
        /// <see cref="System.Threading.Tasks.TaskStatus.RanToCompletion">RanToCompletion</see>, 
        /// <see cref="System.Threading.Tasks.TaskStatus.Faulted">Faulted</see>, or
        /// <see cref="System.Threading.Tasks.TaskStatus.Canceled">Canceled</see>.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="Task"/> was disposed.</exception>
        public void SetResult(TResult result)
        {
            if (!TrySetResult(result))
                throw new InvalidOperationException("Set Result: TaskT_TransitionToFinal_AlreadyCompletd\n" + result);
        }
        public void SetError(Exception error)
        {
            if (!TrySetError(error))
                throw new InvalidOperationException("Set ERROR: TaskT_TransitionToFinal_AlreadyCompleted\n" + error);
        }

        internal void SetIsRunning()
        {
            m_task.SetIsRunning();
        }
    }
}