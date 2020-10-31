using A3;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Azure.Functions.Extensions.Http.Tests
{
    public class MvcModelValueProviderTests
    {
        [Fact]
        public Task ShouldReturnTheValueFromTheValueProvider()
            => A3<MvcModelValueProvider>
            .Arrange(setup =>
            {
                var type = typeof(string);
                var valueProvider = new Func<object>(() => string.Empty);
                setup.Sut(new MvcModelValueProvider(type, valueProvider));
            })
            .Act(async sut => await sut.GetValueAsync().ConfigureAwait(false))
            .Assert(result => result.Should().NotBeNull());
    }
}
