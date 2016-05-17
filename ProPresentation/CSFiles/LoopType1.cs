using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Runtime.Serialization;

namespace proPresentation 
{
    [DataContract]
    public class LoopType1 : LoopType
    {

        public LoopType1() { }

        [DataMember]
        public TimeSpan StartTime;

        [DataMember]
        public TimeSpan EndTime;
        
    }
}
