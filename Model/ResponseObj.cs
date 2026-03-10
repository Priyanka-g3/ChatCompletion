using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCompletion.Model
{
    internal class ResponseObj
    {
        public string? patient_name { get; set; }
        public string? dob { get; set; }
        public string? phone { get; set; }
        public string? gender { get; set; }
    }
}
