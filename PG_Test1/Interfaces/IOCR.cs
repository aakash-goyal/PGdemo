using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PG_Test1.Interfaces
{
    interface IOCR
    {
        JObject getJSON(String Filepath);
        String getProperty(JObject jobject);
    }
}
