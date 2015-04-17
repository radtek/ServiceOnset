﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace ServiceOnset.Config
{
    public interface IServiceOnsetConfig
    {
        bool EnableLog { get; }
        IServiceStartInfo[] StartInfos { get; }
    }
    public partial class ServiceOnsetConfig : IServiceOnsetConfig
    {
        #region Creator

        public static ServiceOnsetConfig Create(string configPath, Encoding encoding)
        {
            string configString;
            using (StreamReader configReader = new StreamReader(configPath))
            {
                configString = configReader.ReadToEnd();
            }
            using (MemoryStream configStream = new MemoryStream(encoding.GetBytes(configString)))
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ServiceOnsetConfig));
                return (ServiceOnsetConfig)jsonSerializer.ReadObject(configStream);
            }
        }
        public static ServiceOnsetConfig Create(string configPath)
        {
            return ServiceOnsetConfig.Create(configPath, Encoding.UTF8);
        }

        #endregion

        private ServiceOnsetConfig()
        {
        }

        private bool? _enableLog;
        public bool EnableLog
        {
            get
            {
                if (!_enableLog.HasValue)
                {
                    _enableLog = _originalEnableLog;
                }
                return _enableLog.Value;
            }
        }

        private IServiceStartInfo[] _startInfos;
        public IServiceStartInfo[] StartInfos
        {
            get
            {
                if (_startInfos == null)
                {
                    if (_originalStartInfos == null)
                    {
                        throw new ArgumentNullException("services");
                    }
                    else
                    {
                        _startInfos = _originalStartInfos.OfType<IServiceStartInfo>().ToArray();
                    }
                }
                return _startInfos;
            }
        }
    }

    public interface IServiceStartInfo
    {
        string Name { get; }
        string Command { get; }
        string Arguments { get; }
        string WorkingDirectory { get; }
        ServiceRunMode RunMode { get; }
        int IntervalInSeconds { get; }
        bool UseShellExecute { get; }
        bool AllowWindow { get; }
        bool KillExistingProcess { get; }
        bool EnableLog { get; }
    }
    public partial class ServiceStartInfo : IServiceStartInfo
    {
        private string _name;
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    if (string.IsNullOrWhiteSpace(_originalName))
                    {
                        throw new ArgumentNullException("name");
                    }
                    else
                    {
                        _name = _originalName;
                    }
                }
                return _name;
            }
        }

        private string _command;
        public string Command
        {
            get
            {
                if (_command == null)
                {
                    if (string.IsNullOrWhiteSpace(_originalCommand))
                    {
                        throw new ArgumentNullException("command");
                    }
                    else if (!Path.IsPathRooted(_originalCommand))
                    {
                        #region 程序路径、工作目录或系统路径

                        string possibleCommand = Path.Combine(AppHelper.AppDirectory, _originalCommand);
                        if (CommandExists(possibleCommand))
                        {
                            _command = possibleCommand;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(_originalWorkingDirectory))
                            {
                                possibleCommand = Path.Combine(_originalWorkingDirectory, _originalCommand);
                                if (CommandExists(possibleCommand))
                                {
                                    _originalCommand = possibleCommand;
                                }
                                else
                                {
                                    //系统路径无需处理，自动应用 Path 环境变量
                                    _command = _originalCommand;
                                }
                            }
                            else
                            {
                                _command = _originalCommand;
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        _command = _originalCommand;
                    }
                }
                return _command;
            }
        }

        private string _arguments;
        public string Arguments
        {
            get
            {
                if (_arguments == null)
                {
                    _arguments = _originalArguments ?? string.Empty;
                }
                return _arguments;
            }
        }

        private string _workingDirectory;
        public string WorkingDirectory
        {
            get
            {
                if (_workingDirectory == null)
                {
                    _workingDirectory = string.IsNullOrWhiteSpace(_originalWorkingDirectory)
                        ? Path.GetDirectoryName(this.Command)
                        : _originalWorkingDirectory;
                }
                return _workingDirectory;
            }
        }

        private ServiceRunMode? _runMode;
        public ServiceRunMode RunMode
        {
            get
            {
                if (!_runMode.HasValue)
                {
                    ServiceRunMode value;
                    if (Enum.TryParse<ServiceRunMode>(_originalRunMode, true, out value))
                    {
                        _runMode = value;
                    }
                    else
                    {
                        _runMode = ServiceRunMode.Daemon;
                    }
                }
                return _runMode.Value;
            }
        }

        private int? _intervalInSeconds;
        public int IntervalInSeconds
        {
            get
            {
                if (!_intervalInSeconds.HasValue)
                {
                    _intervalInSeconds = _originalIntervalInSeconds <= 0
                        ? 30
                        : _originalIntervalInSeconds;
                }
                return _intervalInSeconds.Value;
            }
        }

        private bool? _useShellExecute;
        public bool UseShellExecute
        {
            get
            {
                if (!_useShellExecute.HasValue)
                {
                    _useShellExecute = _originalUseShellExecute;
                }
                return _useShellExecute.Value;
            }
        }

        private bool? _allowWindow;
        public bool AllowWindow
        {
            get
            {
                if (!_allowWindow.HasValue)
                {
                    _allowWindow = _originalAllowWindow;
                }
                return _allowWindow.Value;
            }
        }

        private bool? _killExistingProcess;
        public bool KillExistingProcess
        {
            get
            {
                if (!_killExistingProcess.HasValue)
                {
                    _killExistingProcess = _originalKillExistingProcess;
                }
                return _killExistingProcess.Value;
            }
        }

        private bool? _enableLog;
        public bool EnableLog
        {
            get
            {
                if (!_enableLog.HasValue)
                {
                    _enableLog = _originalEnableLog;
                }
                return _enableLog.Value;
            }
        }

        private bool CommandExists(string command)
        {
            if (File.Exists(command))
            {
                return true;
            }

            string directory = Path.GetDirectoryName(command);
            if (Directory.Exists(directory))
            {
                string fileName = Path.GetFileNameWithoutExtension(command);
                return Directory.GetFiles(directory, fileName + ".*", SearchOption.TopDirectoryOnly).Length > 0;
            }
            else
            {
                return false;
            }
        }
    }
}
