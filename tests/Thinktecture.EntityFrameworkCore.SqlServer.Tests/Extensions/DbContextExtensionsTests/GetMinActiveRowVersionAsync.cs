using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Thinktecture.Extensions.DbContextExtensionsTests
{
   public class GetMinActiveRowVersionAsync : IntegrationTestsBase
   {
      public GetMinActiveRowVersionAsync([NotNull] ITestOutputHelper testOutputHelper)
         : base(testOutputHelper, true)
      {
      }

      [Fact]
      public async Task Should_fetch_min_action_rowversion()
      {
         var rowVersion = await DbContext.GetMinActiveRowVersionAsync(CancellationToken.None).ConfigureAwait(false);
         rowVersion.Should().NotBe(0);
      }
   }
}
