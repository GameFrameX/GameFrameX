namespace Server.Tests;

public class UnitTestTime
{
    private DateTime dateTime, dateTime1;

    [SetUp]
    public void Setup()
    {
        dateTime = DateTime.Now;
        dateTime1 = DateTime.Now.AddHours(1);
    }

    [Test]
    public void Test1()
    {
        Assert.That(dateTime1.Year, Is.EqualTo(dateTime.Year));
        Assert.That(dateTime1.Month, Is.EqualTo(dateTime.Month));
        Assert.That(dateTime1.Day, Is.EqualTo(dateTime.Day));
    }
}