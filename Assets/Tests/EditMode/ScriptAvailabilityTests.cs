using NUnit.Framework;
using System;
using System.Linq;

namespace Zelen.Tests.EditMode
{
    public class ScriptAvailabilityTests
    {
        [TestCase("GlavnoeMenu")]
        [TestCase("DzhoystikNavigatsiyaInventarya")]
        [TestCase("MenuVkladki")]
        [TestCase("Nasazhdenie")]
        [TestCase("VyborNasazhdeniya")]
        public void ScriptIsPresentInProjectAssembly(string typeName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName))
                .FirstOrDefault(t => t != null);

            Assert.IsNotNull(type, "Скрипт " + typeName + " не найден в Assembly-CSharp");
        }
    }
}
