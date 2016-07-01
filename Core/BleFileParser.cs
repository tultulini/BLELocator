using System;
using System.IO;
using System.Runtime.InteropServices;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;

namespace BLELocator.Core
{
    public class BleFileParser : BleMessageParser
    {
        private FileInfo _file;
        private string _fileContent;
        public BleFileParser(string fileName, Action<DeviceDiscoveryEvent> discoveryHandler) : base(discoveryHandler)
        {
            if(fileName.IsNullOrEmpty())
                throw new ArgumentNullException("fileName");

            _file = new FileInfo(fileName);
            if (!_file.Exists)
                throw new Exception(string.Format("Couldn't find {0}", fileName));
            Init();

        }

        private void Init()
        {
            using (var reader = new StreamReader(_file.OpenRead()))
            {
                _fileContent = reader.ReadToEnd();
            }
        }

        public void Start()
        {
            ProcessMessage(_fileContent);
        }
    }
}