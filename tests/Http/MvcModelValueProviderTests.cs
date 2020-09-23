using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Azure.Functions.Extensions.Http.Tests
{
    public class MvcModelValueProviderTests
    {
        [Fact]
        public async Task ShouldReturnTheValueFromTheValueProvider()
        {
            // Arrange
            var type = typeof(string);
            var valueProvider = new Func<object>(() => string.Empty);

            // Assert
            var sut = new MvcModelValueProvider(type, valueProvider);
            var result = await sut.GetValueAsync().ConfigureAwait(false);

            // Act
            result.Should().NotBeNull();
        }
    }
}
