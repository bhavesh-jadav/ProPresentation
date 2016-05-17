using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;

namespace proPresentation
{
    [DataContract(Name = "StoredData")]
    public class StoredData
    {
        private int totalLoops;

        [DataMember]
        public int TotalLoops
        {
            get { return totalLoops; }
            set { totalLoops = Math.Abs(value); }
        }

        [DataMember]
        public string MediaFilePath;

        [DataMember]
        public string DataFilePath;

        private string reverseMediaFilePath;

        public string ReverseMediaFilePath
        {
            get { return reverseMediaFilePath; }
            set { reverseMediaFilePath = value; }
        }


        [DataMember]
        public Dictionary<int, LoopType> loopTypes;
        
    }
}
