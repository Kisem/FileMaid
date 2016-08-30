using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMaid
{
    class FileType
    {
        public string Name;
        public List<string> Extensions = new List<string>();

        public FileType(string name, List<string> extensions)
        {
            Name = name;
            Extensions = extensions;
        }
    }
}
