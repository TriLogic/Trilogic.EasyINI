using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Trilogic.EasyINI
{
    public class IniSetting
    {
        #region Class Members
        protected string key;
        protected string val;

        private List<string> comments = new List<string>();
        #endregion

        #region Constructors and Destructors
        internal IniSetting()
        {
        }
        internal IniSetting(string key, string value)
        {
            Key = key;
            Value = value;
        }
        internal IniSetting(string[] arg)
        {
            Key = arg[0].Trim();
            if (arg.Length > 1)
                Value = arg[1];
        }
        #endregion

        #region Class Properties
        public string Key
        {
            get { return key; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    key = value.Trim();
                if (string.IsNullOrEmpty(key))
                    throw new IniException("invalid setting key");
            }
        }

        public string Value
        {
            get { return val; }
            set { val = value; }
        }

        public List<string> Comments
        {
            get { return comments; }
        }
        #endregion

        #region IO Routines and Overrides
        public int WriteToFile(StreamWriter writer)
        {
            foreach (string comment in comments)
            {
                writer.WriteLine(";{0}", comment);
            }
            writer.WriteLine(this.ToString());
            return 1 + comments.Count;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(val))
                return string.Format("{0}=", key);

            return string.Format("{0}={1}", key, val);
        }
        #endregion
    }

}
