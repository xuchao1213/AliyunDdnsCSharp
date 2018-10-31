/*--------------------------------------------------------
* 
* File: Once
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-31 20:47:23
* Desc: Once
* 
* -------------------------------------------------------*/
using System;
using System.Threading;

namespace AliyunDdnsCSharp.Std
{
    /// <summary>
    /// Once
    /// </summary>
    public class Once : IDisposable
    {
        private long flag;
        private readonly Mutex mutex;

        public Once() {
            flag = 0;
            mutex = new Mutex();
        }

        public void Do(Action func) {
            if (Interlocked.Read(ref flag) == 1)
            {
                return;
            }
            try
            {
                mutex.WaitOne();
                func.Invoke();
                Interlocked.Exchange(ref flag, 1);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        #region IDisposable Support
        private bool disposedValue;

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue)
            {
                if (disposing)
                {
                    mutex.Dispose();
                }
                disposedValue = true;
            }
        }
        ~Once() {
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
