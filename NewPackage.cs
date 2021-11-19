using System;
using System.Collections.Generic;

namespace TestApplication
{
    public class NewPackage : Package
    {
        public NewPackage(Package p)
        {
            this.Id = p.Id;
            this.Name = p.Name;
            this.DateFrom = p.DateFrom;
            this.DateBy = p.DateBy;
            this.DisplayDateFrom = p.DateFrom != DateTime.MinValue ? p.DateFrom.ToShortDateString() : string.Empty;
            this.DisplayDateBy = p.DateBy != DateTime.MaxValue ? p.DateBy.ToShortDateString() : string.Empty; 
            this.Cipher = p.Cipher;
            this.IsExt = default(int);
            this.ExtID = default(int);
        }

        public string DisplayDateFrom { get; set; }
        public string DisplayDateBy { get; set; }

        public int IsExt { get; set; }

        public int ExtID { get; set; }
    }
}
