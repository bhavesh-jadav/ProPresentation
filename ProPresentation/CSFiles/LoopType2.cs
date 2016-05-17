using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using System.Runtime.Serialization;

namespace proPresentation
{
    [DataContract]
    public class LoopType2 : LoopType
    {
        public LoopType2() { }

        [DataMember]
        public TimeSpan FrameTime;
    }
}
