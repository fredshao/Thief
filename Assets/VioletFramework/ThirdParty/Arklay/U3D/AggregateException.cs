using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace U3D
{
    /// <summary>Represents one or more errors that occur during application execution.</summary>
    /// <remarks>
    /// <see cref="AggregateException"/> is used to consolidate multiple failures into a single, throwable
    /// exception object.
    /// </remarks>
    public class AggregateException : Exception
    {
        private List<Exception> m_innerExceptions; // Complete set of exceptions.

        public AggregateException(params Exception[] es) : base("Target invocation caused an exception, check InnerExceptions")
        {
            m_innerExceptions = new List<Exception>(es);
        }

        /// <summary>
        /// Invokes a handler on each <see cref="T:System.Exception"/> contained by this <see
        /// cref="AggregateException"/>.
        /// </summary>
        /// <param name="predicate">The predicate to execute for each exception. The predicate accepts as an
        /// argument the <see cref="T:System.Exception"/> to be processed and returns a Boolean to indicate
        /// whether the exception was handled.</param>
        /// <remarks>
        /// Each invocation of the <paramref name="predicate"/> returns true or false to indicate whether the
        /// <see cref="T:System.Exception"/> was handled. After all invocations, if any exceptions went
        /// unhandled, all unhandled exceptions will be put into a new <see cref="AggregateException"/>
        /// which will be thrown. Otherwise, the <see cref="Handle"/> method simply returns. If any
        /// invocations of the <paramref name="predicate"/> throws an exception, it will halt the processing
        /// of any more exceptions and immediately propagate the thrown exception as-is.
        /// </remarks>
        /// <exception cref="AggregateException">An exception contained by this <see
        /// cref="AggregateException"/> was not handled.</exception>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="predicate"/> argument is
        /// null.</exception>
        public ReadOnlyCollection<Exception> InnerExceptions
        {
            get { return new ReadOnlyCollection<Exception>(m_innerExceptions); }
        }

        public void AddException(Exception e)
        {
            m_innerExceptions.Add(e);
        }

		public override string ToString ()
		{
			if (InnerExceptions.Count == 1) 
			{
				return InnerExceptions[0].ToString();
			} 
			else 
			{
				return string.Format ("[AggregateException: InnerExceptions={0}]", InnerExceptions);
			}
		}
    }
}