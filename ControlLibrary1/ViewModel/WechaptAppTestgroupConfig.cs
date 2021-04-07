using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using AiTest.Utils;
using IniParser;
using IniParser.Model;

namespace AiTest
{
    public class WechaptAppTestgroupConfig : TestGroupSettingsBase
    {
        
        private int _readTimeOut = 6000;
        public int ReadTimeOut
        {
            get { return this._readTimeOut; }
            set
            {
                if (SetProperty(ref this._readTimeOut, value))
                {
                    this.IsDataChanged = true;
                }
            }
        }

        private int _chatRoundCount = 1;
        /// <summary>
        /// 聊天轮数
        /// </summary>
        public int ChatRoundCount
        {
            get { return this._chatRoundCount; }
            set
            {
                if (SetProperty(ref this._chatRoundCount, value))
                {
                    this.IsDataChanged = true;
                }
            }
        }

        private int _chatDelay = 3000;
        public int ChatDelay
        {
            get { return this._chatDelay; }
            set
            {
                if (SetProperty(ref this._chatDelay, value))
                {
                    this.IsDataChanged = true;
                }
            }
        }
         
 
        public readonly Dictionary<string, string> OpenIdIdentifyMap = new Dictionary<string, string>();

        private string _userDataFileName;
        public string UserDataFileName
        {
            get
            {
                
                return this._userDataFileName;
            }
            set
            {
              
                if (SetProperty(ref this._userDataFileName, value))
                {
                   
                    this.IsDataChanged = true;
                }

            }
        }

        public void UpdateUserDataFileName(string fileName)
        {

            LoadUserData(fileName);
            this.UserDataFileName = fileName;

        }

        private void LoadUserData(string fileName)
        { 
            if (!File.Exists(fileName))
            {
                return;
            }
             
            this.OpenIdIdentifyMap.Clear();
            var lines = File.ReadAllLines(fileName);

            foreach (var line in lines)
            {
                var arr = line.Split(',');
                this.OpenIdIdentifyMap.Add(arr[0].Trim(), arr[1].Trim());
            }
             
        }

  
         


         

        public WechaptAppTestgroupConfig()
        {
        }

        protected override void DoSaveToXaml(XElement element)
        {
            base.DoSaveToXaml(element);

            element.SetAttributeValue("UserDataCsvFileName", this.UserDataFileName);

            element.SetAttributeValue("ChatDelay", this.ChatDelay);
             

            element.SetAttributeValue("ChatRoundCount", this.ChatRoundCount);
 
            element.SetAttributeValue("ReadTimeOut", this.ReadTimeOut);
        }

        protected override void DoLoadFromXaml(XElement element)
        {
            base.DoLoadFromXaml(element);
            this.ChatDelay = (int)element.Attribute("ChatDelay");
            this.ChatRoundCount = (int)element.Attribute("ChatRoundCount");
            UpdateUserDataFileName(element.Attribute("UserDataCsvFileName")?.Value);
            this.ReadTimeOut = (int)element.Attribute("ReadTimeOut");
        }
 
    }
}