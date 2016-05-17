using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace proPresentation
{
    [DataContract]
    [KnownType(typeof(LoopType1))]
    [KnownType(typeof(LoopType2))]
    public class LoopType
    {
        public LoopType() { }
    }
}
