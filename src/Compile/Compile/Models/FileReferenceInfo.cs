using AJ.Common.Helpers;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Compile.Models
{
    public class FileReferenceInfo
    {
        public string StandardFile { get; set; }
        public IEnumerable<string> ReferenceFile => _referenceFiles.ToArray();

        public FileReferenceInfo(string standardFile)
        {
            StandardFile = standardFile;
        }

        public FileReferenceInfo(string standardFile, StringCollection referenceFiles) : this(standardFile)
        {
            _referenceFiles = referenceFiles;
        }

        private StringCollection _referenceFiles = new StringCollection();
    }
}
