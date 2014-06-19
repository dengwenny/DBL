using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBL.Mapping
{
    [Serializable]
    public class MappingException : Exception
    {
        public MappingException(string message) { }
        public MappingException(string message, Exception innerException) { }
    }
}
