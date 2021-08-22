using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Logs Task exceptions.
        /// Tasks that are not awaited default don't show any trace of an exception in the Unity Console
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task LogErrors(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Logs Task exceptions.
        /// Tasks that are not awaited default don't show any trace of an exception in the Unity Console
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<T> LogErrors<T>(this Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
        
    }
}
