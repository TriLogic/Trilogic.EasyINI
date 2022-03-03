using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Trilogic.EasyINI
{
    public class IniSettings
    {
        #region Class Members
        protected Dictionary<string, IniGroup> formalGroups = new Dictionary<string,IniGroup>();
        protected IniGroup globalGroup = new IniGroup();
        protected List<string> trailerComments = new List<string>();
        #endregion

        #region Constructors and Destructors
        public IniSettings()
        {
        }
        #endregion

        #region Class Properties
        public IniGroup Globals
        {
            get { return globalGroup; }
        }

        public Dictionary<string, IniGroup> FormalGroups
        {
            get { return formalGroups; }
        }

        public List<string> TrailerComments
        {
            get { return trailerComments; }
        }
        #endregion

        #region Read and Write Settings
        public static IniSettings ReadSettings(string path)
        {
            IniSettings ini = new IniSettings();
            ini.Read(path);
            return ini;
        }

        public void Write(string path)
        {
            StreamWriter writer = new StreamWriter(path, false);
            Write(writer);
            writer.Close();
            writer.Flush();
        }

        public void Write(StreamWriter writer)
        {
            // write out a defaults group
            int written = globalGroup.Write(writer);
            if (written > 0)
                writer.WriteLine();

            // Write group values
            foreach (IniGroup g in formalGroups.Values)
            {
                int iwritten = g.Write(writer);
                if (iwritten > 0)
                    writer.WriteLine();
                written += iwritten;
            }

            // Write trailer comments
            foreach (string comment in trailerComments)
                writer.WriteLine(";{0}", comment);
        }

        #endregion

        #region Default Group Behavior
        public bool Exists(string key)
        {
            return globalGroup.Exists(key);
        }
        public IniSetting Get(string key)
        {
            return globalGroup.Get(key);
        }
        public IniSetting Set(string key, string value)
        {
            return globalGroup.Set(key, value);
        }
        public IniSetting Remove(string key)
        {
            return globalGroup.Remove(key);
        }
        #endregion

        #region Specific Group Behavior
        public bool Exists(string group, string key)
        {
            if (string.IsNullOrEmpty(group))
                return globalGroup.Exists(key);
            return GetGroup(group).Exists(key);
        }
        public IniSetting Get(string group, string key)
        {
            if (string.IsNullOrEmpty(group))
                return globalGroup.Get(key);
            return GetGroup(group).Get(key);
        }
        public IniSetting Get(string group, string key, string defaultValue)
        {
            if (!HasGroup(group))
                return AddGroup(group).Set(key, defaultValue);

            return GetGroup(group).Set(key, defaultValue);
        }
        public IniSetting Set(string group, string key, string value)
        {
            if (!HasGroup(group))
                AddGroup(group);
            return GetGroup(group).Set(key, value);
        }
        public IniSetting Remove(string group, string key)
        {
            return GetGroup(group).Remove(key);
        }
        #endregion

        #region Group Management
        public bool HasGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
                return false;
            string lowkey = group.ToLower();
            return formalGroups.ContainsKey(lowkey);
        }

        public IniGroup AddGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
                throw new IniException("invalid group name");

            string lowkey = group.ToLower();
            if (formalGroups.ContainsKey(lowkey))
                throw new Exception(string.Format("group already exists [{0}]", group));

            IniGroup g = new IniGroup(group);
            formalGroups.Add(lowkey, g);

            return g;
        }

        public IniGroup RemoveGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
                throw new IniException("invalid group name");

            string lowkey = group.ToLower();
            if (!formalGroups.ContainsKey(lowkey))
                throw new Exception(string.Format("group not found [{0}]", group));

            IniGroup g = formalGroups[lowkey];
            formalGroups.Remove(lowkey);

            return g;
        }

        public IniGroup GetGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
                throw new IniException("invalid group name");

            string lowkey = group.ToLower();
            if (!formalGroups.ContainsKey(lowkey))
                throw new Exception(string.Format("group not found [{0}]", group));

            return formalGroups[lowkey];
        }
        #endregion

        #region Reading Files
        public void Read(StreamReader reader)
        {
            // delete existing groups
            formalGroups.Clear();

            List<string> comments = new List<string>();
            IniGroup curr = globalGroup;

            while (!reader.EndOfStream)
            {
                string input = reader.ReadLine().Trim();

                if (input.Length < 1)
                    continue;

                if (input[0] == ';')
                {
                    if (input.Length == 1)
                        input = string.Empty;
                    else
                        input = input.Substring(1).TrimEnd();

                    comments.Add(input);
                    continue;
                }

                if (input[0] == '[')
                {
                    curr = StartGroup(input);
                    formalGroups.Add(curr.Name, curr);

                    foreach (string comment in comments)
                        curr.Comments.Add(comment);

                    comments.Clear();
                    continue;
                }

                IniSetting setting = ParseSetting(input);
                curr.Set(setting);

                foreach (string comment in comments)
                    setting.Comments.Add(comment);

                comments.Clear();
            }

            // trailer comments
            if (comments.Count > 0)
                foreach (string comment in comments)
                    trailerComments.Add(comment);

            reader.Close();
        }

        public void Read(string path)
        {
            Read(File.OpenText(path));
        }

        private IniSetting ParseSetting(string input)
        {
            int idxEq = input.IndexOf('=');
            if (idxEq < 1)
                throw new IniException("invalid ini setting");

            string k = input.Substring(0, idxEq);
            string v = string.Empty;

            if (input.Length > idxEq)
            {
                v = input.Substring(idxEq + 1);
            }
            k = k.Trim();
            v = v.Trim();

            if (k.Length < 1)
                throw new IniException("invalid ini setting");

            return new IniSetting(k, v);
        }

        private IniGroup StartGroup(string input)
        {
            int idx = input.IndexOf(']');
            if ( idx < 2 )
                throw new IniException("invalid group tag");

            string name = input.Substring(1, idx-1).Trim();
            IniGroup g = new IniGroup(name);
            return g;
        }
        #endregion
    }
}
