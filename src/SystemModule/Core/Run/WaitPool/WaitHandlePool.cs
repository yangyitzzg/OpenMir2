using System.Collections.Concurrent;
using System.Threading;
using SystemModule.Core.Common;
using SystemModule.Extensions;

namespace SystemModule.Core.Run.WaitPool
{
    /// <summary>
    /// 等待处理数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WaitHandlePool<T> : DisposableObject where T : IWaitResult
    {
        private readonly ConcurrentDictionary<long, WaitData<T>> m_waitDic;
        private readonly ConcurrentQueue<WaitData<T>> m_waitQueue;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitHandlePool()
        {
            m_waitDic = new ConcurrentDictionary<long, WaitData<T>>();
            m_waitQueue = new ConcurrentQueue<WaitData<T>>();
        }

        /// <summary>
        /// 销毁
        /// </summary>
        /// <param name="waitData"></param>
        public void Destroy(WaitData<T> waitData)
        {
            if (waitData.DisposedValue)
            {
                return;
            }
            if (m_waitDic.TryRemove(waitData.WaitResult.Sign, out _))
            {
                waitData.Reset();
                m_waitQueue.Enqueue(waitData);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            foreach (WaitData<T> item in m_waitDic.Values)
            {
                item.SafeDispose();
            }
            foreach (WaitData<T> item in m_waitQueue)
            {
                item.SafeDispose();
            }
            m_waitDic.Clear();

            m_waitQueue.Clear();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 取消全部
        /// </summary>
        public void CancelAll()
        {
            foreach (WaitData<T> item in m_waitDic.Values)
            {
                item.Cancel();
            }
        }

        /// <summary>
        /// 延迟模式
        /// </summary>
        public bool DelayModel { get; set; } = false;

        private long m_waitCount;
        private long m_waitReverseCount;

        /// <summary>
        ///  获取一个可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitData<T> GetWaitData(T result, bool autoSign = true)
        {
            if (m_waitQueue.TryDequeue(out WaitData<T> waitData))
            {
                if (autoSign)
                {
                    result.Sign = Interlocked.Increment(ref m_waitCount);
                }
                waitData.SetResult(result);
                m_waitDic.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitData<T>();
            waitData.DelayModel = DelayModel;
            if (autoSign)
            {
                result.Sign = Interlocked.Increment(ref m_waitCount);
            }
            waitData.SetResult(result);
            m_waitDic.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        ///  获取一个Sign为负数的可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitData<T> GetReverseWaitData(T result, bool autoSign = true)
        {
            if (m_waitQueue.TryDequeue(out WaitData<T> waitData))
            {
                if (autoSign)
                {
                    result.Sign = Interlocked.Decrement(ref m_waitReverseCount);
                }
                waitData.SetResult(result);
                m_waitDic.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitData<T>();
            waitData.DelayModel = DelayModel;
            if (autoSign)
            {
                result.Sign = Interlocked.Decrement(ref m_waitReverseCount);
            }
            waitData.SetResult(result);
            m_waitDic.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="sign"></param>
        public void SetRun(long sign)
        {
            if (m_waitDic.TryGetValue(sign, out WaitData<T> waitData))
            {
                waitData.Set();
            }
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="waitResult"></param>
        public void SetRun(long sign, T waitResult)
        {
            if (m_waitDic.TryGetValue(sign, out WaitData<T> waitData))
            {
                waitData.Set(waitResult);
            }
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="waitResult"></param>
        public void SetRun(T waitResult)
        {
            if (m_waitDic.TryGetValue(waitResult.Sign, out WaitData<T> waitData))
            {
                waitData.Set(waitResult);
            }
        }
    }
}