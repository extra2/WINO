using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINO
{
    [Serializable]
    public class Term
    {
        public bool iKnowThat { get; set; }
        public string term { get; set; }
        public string tag { get; set; }
        public string definition { get; set; }

        public Term()
        {
            iKnowThat = false;
            term = "";
            tag = "";
            definition = "";
        }

        public string generate(int mode)
        {
            return "";
        }
    }
}
