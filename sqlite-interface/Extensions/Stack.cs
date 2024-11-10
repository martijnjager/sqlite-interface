using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Database.Extensions
{
    public static class Stack
    {
        public static MethodBase GetPreviousMethod()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            var frames = stackTrace.GetFrames();
            var secondToLastFrame = frames[2];
            var method = secondToLastFrame.GetMethod();
            return method;
        }
    }
}
