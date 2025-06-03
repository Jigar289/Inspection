using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inspection.Web.Models
{
    public class HoldProcessModel
    {
        public int ID { get; set; }
        public string JobNum { get; set; }
        public string PartNum { get; set; }
        public string Inspection_Type { get; set; }
        public DateTime? Inspection_date { get; set; }
        public string starttime { get; set; }
        public string endtime { get; set; }
        public int? Inspection_Qty { get; set; }
        public string Qualitystage { get; set; }
        public int? sampleqty { get; set; }
    }

}