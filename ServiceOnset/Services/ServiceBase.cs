﻿using log4net;
using ServiceOnset.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ServiceOnset.Services
{
    public abstract class ServiceBase : IService
    {
        #region IDisposable

        private bool disposed = false;
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    #region Dispose managed resources

                    if (this.IsRunning)
                    {
                        this.IsRunning = false;
                    }
                    if (this.InnerProcess != null)
                    {
                        try
                        {
                            if (!this.InnerProcess.HasExited)
                            {
                                this.InnerProcess.Kill();
                            }
                            this.InnerProcess.Dispose();
                        }
                        catch
                        {
                        }
                        finally
                        {
                            this.InnerProcess = null;
                        }
                    }
                    if (this.InnerThread != null)
                    {
                        try
                        {
                            if (this.InnerThread.IsAlive)
                            {
                                this.InnerThread.Abort();
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            this.InnerThread = null;
                        }
                    }
                    this.InnerLogger = null;
                    this.StartInfo = null;

                    #endregion
                }

                #region Clean up unmanaged resources

                //

                #endregion

                disposed = true;
            }
        }

        ~ServiceBase()
        {
            this.Dispose(false);
        }

        #endregion

        public IServiceStartInfo StartInfo
        {
            get;
            private set;
        }
        protected ILog InnerLogger
        {
            get;
            private set;
        }

        public Process InnerProcess
        {
            get;
            private set;
        }
        protected Thread InnerThread
        {
            get;
            private set;
        }
        protected bool IsRunning
        {
            get;
            set;
        }

        public ServiceBase(IServiceStartInfo startInfo)
        {
            this.StartInfo = startInfo;
            this.InnerLogger = LogManager.GetLogger(startInfo.Name);

            this.InnerProcess = new Process();
            this.InnerProcess.StartInfo.UseShellExecute = false;
            this.InnerProcess.StartInfo.ErrorDialog = false;
            this.InnerProcess.StartInfo.CreateNoWindow = true;
            this.InnerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            this.InnerProcess.ErrorDataReceived += InnerProcess_ErrorDataReceived;
            this.InnerProcess.OutputDataReceived += InnerProcess_OutputDataReceived;
            this.InnerProcess.StartInfo.RedirectStandardError = true;
            this.InnerProcess.StartInfo.RedirectStandardOutput = this.StartInfo.LogOutput;

            this.InnerThread = new Thread(new ThreadStart(this.ThreadProc));
            this.InnerThread.IsBackground = true;

            this.IsRunning = false;
        }

        #region Process events

        private void InnerProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.InnerLogger.ErrorFormat("Process [{0}] error: {1}", this.StartInfo.Name, e.Data);
        }
        private void InnerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.InnerLogger.InfoFormat("Process [{0}] output: {1}", this.StartInfo.Name, e.Data);
        }

        #endregion

        public virtual void Start()
        {
            this.IsRunning = true;
            this.InnerThread.Start();
            this.InnerLogger.InfoFormat("Thread [{0}] started", this.StartInfo.Name);
        }
        protected abstract void ThreadProc();
    }
}
