using A3;
using AutoFixture.Xunit2;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace Azure.Functions.Extensions.OData.Tests
{
    public class ODataBindingValueProviderTests
    {
        [Theory]
        [AutoData]
        public Task ShouldReturnTheValueFromTheValueProvider(string input)
            => A3<ODataBindingValueProvider>
            .Arrange(setup => setup.Sut(new ODataBindingValueProvider(input)))
            .Act(async sut => await sut.GetValueAsync().ConfigureAwait(false))
            .Assert(result => result.Should().Be(input));
    }
}
