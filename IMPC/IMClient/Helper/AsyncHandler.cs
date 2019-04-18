/*
Author: TanQiYan
Create date: 2018-08-20
Description：异步处理类，本类用于统一的提供异步调用。
-------------------------------------------------------------------------------------
Versions：
    V1.00 2018-08-20 TanQiYan Create
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace IMClient.Helper
{
    public static class AsyncHandler
    {
        /// <summary>
        /// 异步处理类，本类用于统一的提供异步调用。
        /// </summary>
        static AsyncHandler() { }

        class AsyncArgs
        {
            public object Handler;
            public object Callback;
            public object Args;
            public Dispatcher Dispatcher;
        }
        /// <summary>
        /// 调用指定的方法
        /// </summary>
        /// <param name="handler">指向方法的委托</param>
        /// <param name="callback">在异步调用后的回调通知（如果出错，则异常参数不为空）</param>
        public static void Call(Action handler, Action<Exception> callback = null)
        {
            if (handler != null)
            {
                AsyncArgs state = new AsyncArgs();
                state.Handler = handler;
                state.Callback = callback;
                ThreadPool.QueueUserWorkItem(o =>
                {
                    AsyncArgs tmpArgs = (AsyncArgs)o;
                    try
                    {
                        ((Action)tmpArgs.Handler)();
                        if (tmpArgs.Callback != null) ((Action<Exception>)tmpArgs.Callback)(null);
                    }
                    catch (Exception ex)
                    {
                        if (tmpArgs.Callback != null) ((Action<Exception>)tmpArgs.Callback)(ex);
                    }
                    tmpArgs = null;
                }, state);
            }
        }

        /// <summary>
        /// 调用指定的函数，并有返回值
        /// </summary>
        /// <typeparam name="TResult">函数返回的类型信息</typeparam>
        /// <param name="dispatcher">当前UI调度器，若未指定则自动创建一个</param>
        /// <param name="handler">指向方法的委托</param>
        /// <param name="callback">在异步调用后的回调通知（如果出错，则异常参数不为空），本回调将在UI线程中运行，可以直接操作UI控件。</param>
        public static void CallFuncWithUI<TResult>(Dispatcher dispatcher, Func<TResult> handler, Action<Exception, TResult> callback = null)
        {
            if (dispatcher == null)
            {
                dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                if (dispatcher == null) throw new ArgumentNullException("dispatcher", "未指定UI调度器。");
            }
            if (handler != null)
            {
                AsyncArgs state = new AsyncArgs();
                state.Handler = handler;
                state.Callback = callback;
                state.Dispatcher = dispatcher;
                ThreadPool.QueueUserWorkItem(o =>
                {
                    AsyncArgs tmpArgs = (AsyncArgs)o;
                    try
                    {
                        TResult result = ((Func<TResult>)tmpArgs.Handler)();
                        if (tmpArgs.Callback != null)
                        {   //在UI线程中调用
                            object[] methodArgs = new object[] { null, result };
                            tmpArgs.Dispatcher.BeginInvoke((Action<Exception, TResult>)tmpArgs.Callback, methodArgs);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (tmpArgs.Callback != null)
                        {   //在UI线程中调用
                            object[] methodArgs = new object[] { ex, default(TResult) };
                            tmpArgs.Dispatcher.BeginInvoke((Action<Exception, TResult>)tmpArgs.Callback, methodArgs);
                        }
                    }
                    tmpArgs = null;
                }, state);
            }
        }

        /// <summary>
        /// 在UI线程中直接调用方法
        /// </summary>
        /// <param name="dispatcher">调度器</param>
        /// <param name="handler">指向方法的委托</param>
        public static void AsyncCall(Dispatcher dispatcher, Action handler, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (dispatcher != null && handler != null)
            {
                dispatcher.BeginInvoke(priority, handler);
            }
        }

        /// <summary>
        /// 在UI线程中直接调用方法
        /// </summary>
        /// <param name="dispatcher">调度器</param>
        /// <param name="handler">指向方法的委托</param>
        public static void Call(Dispatcher dispatcher, Action handler, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (dispatcher != null && handler != null)
            {
                try
                {
                    dispatcher.Invoke(priority, handler);
                }
                catch (ThreadAbortException)
                {   //此类异常直接吞
                }
            }
        }
    }
}
