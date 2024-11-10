using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Test.Extension
{
    public static class InstanceManagerTest
    {
        public static void Setup(this InstanceContainer instance, object objectToMock)
        {
            InstanceContainer.Instance.SetSingleton(objectToMock, objectToMock.GetType().Name);
        }
    }
}
