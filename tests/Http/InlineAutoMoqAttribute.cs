using AutoFixture.Xunit2;
using System;

namespace Azure.Functions.Extensions.Http.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InlineAutoMoqAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMoqAttribute(params object[] values)
            : base(new AutoMoqAttribute(), values)
        {
        }
    }
}
