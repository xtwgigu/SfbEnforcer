using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Diagnostics;

namespace SfbHttpEnforcer
{
    class CHttpParserPersistentChat : CHttpParserBase
    {
        public CHttpParserPersistentChat()
        {
        }

        protected override ResponseFilter CreateResponseFilter(HttpResponse httpResponse)
        {
            return new ResponseFilterPersistentChat(httpResponse);
        }

    }
}
