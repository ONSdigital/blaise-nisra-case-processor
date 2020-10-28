using System;

namespace Blaise.Nisra.Case.Processor.Tests.Behaviour.Helpers
{
    public class EnumHelper
    {
        public static T Parse<T>(string enumValue)
        {
            return (T) Enum.Parse(typeof(T), enumValue, true);
        }
    }
}
