using UnityEngine;
using UnityEngine.Profiling;

namespace Offworld.SystemCore
{
    //example:
    //class Foo
    //{
    //    public void Example()
    //    {
    //        using (new ProfilePrintScope("ExpensiveStuff"))
    //        {
    //            //stuff...
    //        }
    //    }
    //}

    public struct ProfilePrintScope : System.IDisposable
    {        
        public ProfilePrintScope(string name)
        {
        }
        
        public void Dispose()
        {
        }
    }

    //--------------------------------------------------------------------------------------------------

    //example:
    //class Foo
    //{
    //    public void Example()
    //    {
    //        using (new UnityProfileScope("ExpensiveStuff"))
    //        {
    //            //stuff...
    //        }
    //    }
    //}

    public struct UnityProfileScope : System.IDisposable
    {
        public UnityProfileScope(string name)
        {
        }

        public void Dispose()
        {
        }
    }
}
