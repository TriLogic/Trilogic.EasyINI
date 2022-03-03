using System;

namespace Trilogic.EasyINI
{
    public class IniException : Exception
    {
        public IniException(string msg)
            : base(msg)
        {
        }
    }
}
