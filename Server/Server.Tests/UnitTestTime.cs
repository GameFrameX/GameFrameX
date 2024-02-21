using System;
using NUnit.Framework;
using Server.Utility;

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
    public void test_current_time_millis()
    {
        long currentTimeMillis = TimeHelper.CurrentTimeMillis();
        long expectedTimeMillis = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        Assert.AreEqual(currentTimeMillis, (expectedTimeMillis));
    }

    [Test]
    public void test_unix_time_seconds()
    {
        long unixTimeSeconds = TimeHelper.UnixTimeSeconds();
        long expectedUnixTimeSeconds = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        Assert.AreEqual(expectedUnixTimeSeconds, unixTimeSeconds);
    }

    [Test]
    public void test_unix_time_milliseconds()
    {
        long unixTimeMilliseconds = TimeHelper.UnixTimeMilliseconds();
        long expectedUnixTimeMilliseconds = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        Assert.AreEqual(expectedUnixTimeMilliseconds, unixTimeMilliseconds);
    }

    [Test]
    public void test_time_millis()
    {
        DateTime time = new DateTime(2022, 1, 1, 12, 0, 0);
        bool utc = false;
        long timeMillis = TimeHelper.TimeMillis(time, utc);
        long expectedTimeMillis = (long)(time - new DateTime(1970, 1, 1)).TotalMilliseconds;
        Assert.AreEqual(expectedTimeMillis, timeMillis);
    }

    [Test]
    public void test_millis_to_date_time()
    {
        long timeMillis = 1641024000000;
        bool utc = true;
        DateTime dateTime = TimeHelper.MillisToDateTime(timeMillis, utc);
        DateTime expectedDateTime = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.AreEqual(expectedDateTime, dateTime);
    }

    [Test]
    public void test_get_cross_days()
    {
        DateTime begin = new DateTime(2022, 3, 12, 1, 0, 0);
        DateTime after = new DateTime(2022, 3, 13, 23, 0, 0);
        int hour = 0;
        int crossDays = TimeHelper.GetCrossDays(begin, after, hour);
        int expectedCrossDays = 1;
        Assert.AreEqual(expectedCrossDays, crossDays);
    }

    [Test]
    public void Test1()
    {
        Assert.That(dateTime1.Year, Is.EqualTo(dateTime.Year));
        Assert.That(dateTime1.Month, Is.EqualTo(dateTime.Month));
        Assert.That(dateTime1.Day, Is.EqualTo(dateTime.Day));
    }
}