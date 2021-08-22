using System;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;

namespace Assets
{
    public class ThreadedMonoBehaviour : MonoBehaviour
    {
        private static readonly CancellationTokenSource TokenSource = new CancellationTokenSource();
        protected static TaskFactory taskFactory;
        protected static TaskScheduler ctx;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnLoad()
        {
            ctx = TaskScheduler.FromCurrentSynchronizationContext();
            taskFactory = new TaskFactory(
                TokenSource.Token, 
                TaskCreationOptions.DenyChildAttach,
                TaskContinuationOptions.DenyChildAttach,
                ctx);
            Application.quitting += OnQuit;
        }

        private static void OnQuit()
        {
            TokenSource.Cancel();
            TokenSource.Dispose();
        }
        
        protected static Task Delay(TimeSpan delay) => Task.Delay(delay, TokenSource.Token);
        protected static Task Delay(TimeSpan delay, CancellationToken token) => Task.Delay(delay, token);
        protected static Task Delay(int millisecondsDelay) => Task.Delay(millisecondsDelay, TokenSource.Token);
        protected static Task Delay(int millisecondsDelay, CancellationToken token) => Task.Delay(millisecondsDelay, token);

        #region RunThreaded(...)
        /// <summary>
        /// Runs on a separate Thread
        /// </summary>
        protected static Task RunThreaded(Action action) => 
            Task.Run(action, TokenSource.Token);
        /// <summary>
        /// Runs on a separate Thread
        /// </summary>
        protected static Task RunThreaded(Func<Task> function) =>
            Task.Run(function, TokenSource.Token);
        /// <summary>
        /// Runs on a separate Thread
        /// </summary>
        protected static Task<TResult> RunThreaded<TResult>(Func<TResult> function) => 
            Task.Run(function, TokenSource.Token);
        /// <summary>
        /// Runs on a separate Thread
        /// </summary>
        protected static Task<TResult> RunThreaded<TResult>(Func<Task<TResult>> function) => 
            Task.Run(function, TokenSource.Token);
        /// <summary>
        /// Runs on a separate Thread
        /// </summary>
        protected static Task RunThreaded(Action action, CancellationToken token) =>
            Task.Run(action, token);
        /// <summary>
        /// Runs on a separate Thread
        /// </summary>
        protected static Task RunThreaded(Func<Task> function, CancellationToken token) =>
            Task.Run(function, token);
        /// <summary>
        /// Runs on a separate Thread
        /// </summary>
        protected static Task<TResult> RunThreaded<TResult>(Func<TResult> function, CancellationToken token) =>
            Task.Run(function, token);
        /// <summary>
        /// Runs on a separate Thread
        /// </summary>
        protected static Task<TResult> RunThreaded<TResult>(Func<Task<TResult>> function, CancellationToken token) =>
            Task.Run(function, token);

        #endregion

        #region RunMain(...)

        /// <summary>
        /// Runs on the Main Unity Thread
        /// </summary>
        protected static async Task RunMain(Action action) => 
            await taskFactory.StartNew(action);

        /// <summary>
        /// Runs on the Main Unity Thread
        /// </summary>
        protected static async Task<T> RunMain<T>(Func<T> action) =>
            await taskFactory.StartNew(action);

        /// <summary>
        /// Runs on the Main Unity Thread
        /// </summary>
        protected static async Task RunMain(Func<Task> action) =>
            await await taskFactory.StartNew(action);

        /// <summary>
        /// Runs on the Main Unity Thread
        /// </summary>
        protected static async Task<T> RunMain<T>(Func<Task<T>> action) =>
            await await taskFactory.StartNew(action);

        /// <summary>
        /// Runs on the Main Unity Thread
        /// </summary>
        protected static async Task RunMain(Action<CancellationToken> action) =>
            await taskFactory.StartNew(token => action((CancellationToken) token), TokenSource.Token);

        /// <summary>
        /// Runs on the Main Unity Thread
        /// </summary>
        protected static async Task<T> RunMain<T>(Func<CancellationToken, T> action) =>
            await taskFactory.StartNew(token => action((CancellationToken) token), TokenSource.Token);

        /// <summary>
        /// Runs on the Main Unity Thread
        /// </summary>
        protected static async Task RunMain(Func<CancellationToken, Task> action) =>
            await await taskFactory.StartNew(token => action((CancellationToken) token), TokenSource.Token);

        /// <summary>
        /// Runs on the Main Unity Thread
        /// </summary>
        protected static async Task<T> RunMain<T>(Func<CancellationToken, Task<T>> action) => 
            await await taskFactory.StartNew(token => action((CancellationToken) token), TokenSource.Token);

        #endregion
    }
}