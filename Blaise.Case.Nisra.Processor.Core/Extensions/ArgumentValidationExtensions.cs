using System;

namespace Blaise.Case.Nisra.Processor.Core.Extensions
{
       public static class ArgumentValidationExtensions
    {
        public static void ThrowExceptionIfNullOrEmpty(this string argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }

            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException($"A value for the argument '{argumentName}' must be supplied");
            }
        }
    }
}