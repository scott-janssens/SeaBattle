using SeaBattleEngine;
using System.Diagnostics.CodeAnalysis;

namespace SeaBattleEngineTests;

[ExcludeFromCodeCoverage]
[TestFixture]
public class PlayerTests
{
    [Test]
    public void PlayerCtor_Success()
    {
        var actual = new Player("name", "id");

        Assert.Multiple(() =>
        {
            Assert.That(actual.Name, Is.EqualTo("name"));
            Assert.That(actual.Id, Is.EqualTo("id"));
            Assert.That(actual.Map, Is.Not.Null);
        });
    }

    [Test]
    public void PlayerCtor_Failure()
    {
        Assert.Throws<ArgumentException>(() => new Player(string.Empty, string.Empty));
    }
}