using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.UnityThreading
{
    public class ThreadingTestScript : ThreadedMonoBehaviour
    {
        private Stopwatch stopWatch;


        private void Awake()
        {

            stopWatch = Stopwatch.StartNew();
        }
        // Start is called before the first frame update
        private void Start()
        {
            //this.ctx 
            Debug.Log("start Start()");
            LogThread();

            //string runThreaded = await
            _ = RunThreaded(MethodA).LogErrors();
        
            Debug.Log("end Start()");
        }

        public async Task<string> MethodA() // threaded
        {
            Debug.Log("start MethodA()");
            LogThread();
            await Delay(5000);


            string runMain = await RunMain((token) => SetPositionAsync(Vector3.forward, token));
        

            Debug.Log("end MethodA()");
            return "method A output";
        }

        public async Task<string> SetPositionAsync(Vector3 position, CancellationToken token = default)
        {
            Debug.Log("start SetPositionAsync");
            LogThread();

            await Delay(5000, token);
            await Task.Yield();

            Debug.Log("delayed SetPositionAsync");

            gameObject.transform.position = position;

            Debug.Log("end SetPositionAsync");
            return "output pos";
        }


        public void LogThread() => Debug.Log($"{stopWatch.ElapsedMilliseconds} Current Thread-Id: {Thread.CurrentThread.ManagedThreadId}");
    }
}
