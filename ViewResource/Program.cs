using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Globalization;
using System.Diagnostics;

namespace ViewResource
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Reflection.Assembly myOtherAssembly = myOtherAssembly = System.Reflection.Assembly.Load("RtcMgmt.Resource");
            
            //"RtcMgmt.Resource.UIResource.resources"
            ResourceManager resMgr = new ResourceManager("UIResource", myOtherAssembly);

            ResourceSet resSet = resMgr.GetResourceSet(CultureInfo.CreateSpecificCulture("en-us"), true, false);

            foreach(var res in resSet)
            {
                Trace.WriteLine(res.ToString());
            }

        }
    }
}
