using System;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Helpers
{
    public class EnumHelper
    {
        public static T Parse<T>(string enumValue)
        {
            return (T) Enum.Parse(typeof(T), enumValue, true);
        }
    }
}
