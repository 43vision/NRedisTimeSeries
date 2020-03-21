﻿using System;
using System.Collections.Generic;
using System.Threading;
using NRedisTimeSeries.DataTypes;
using StackExchange.Redis;
using Xunit;

namespace NRedisTimeSeries.Test.TestAPI
{
    public class TestAdd : AbstractTimeSeriesTest, IDisposable
    {
        private readonly string key = "ADD_TESTS";

        public TestAdd(RedisFixture redisFixture) : base(redisFixture) { }

        public void Dispose()
        {
            redisFixture.redis.GetDatabase().KeyDelete(key);
        }

        [Fact]
        public void TestAddNotExistingTimeSeries()
        {
            IDatabase db = redisFixture.redis.GetDatabase();
            TimeStamp now = DateTime.Now;
            Assert.Equal(now, db.TimeSeriesAdd(key, now, 1.1));
            TimeSeriesInformation info = db.TimeSeriesInfo(key);
            Assert.Equal(now, info.FirstTimeStamp);
            Assert.Equal(now, info.LastTimeStap);
        }

        [Fact]
        public void TestAddExistingTimeSeries()
        {
            IDatabase db = redisFixture.redis.GetDatabase();
            db.TimeSeriesCreate(key);
            TimeStamp now = DateTime.Now;
            Assert.Equal(now, db.TimeSeriesAdd(key, now, 1.1));
            TimeSeriesInformation info = db.TimeSeriesInfo(key);
            Assert.Equal(now, info.FirstTimeStamp);
            Assert.Equal(now, info.LastTimeStap);
        }

        [Fact]
        public void TestAddStar()
        {
            IDatabase db = redisFixture.redis.GetDatabase();
            db.TimeSeriesAdd(key, "*", 1.1);
            TimeSeriesInformation info = db.TimeSeriesInfo(key);
            Assert.True(info.FirstTimeStamp > 0);
            Assert.Equal(info.FirstTimeStamp, info.LastTimeStap);
        }

        [Fact]
        public void TestAddRetentionTime()
        {
            IDatabase db = redisFixture.redis.GetDatabase();
            TimeStamp now = DateTime.Now;
            long retentionTime = 5000;
            Assert.Equal(now, db.TimeSeriesAdd(key, now, 1.1, retentionTime: retentionTime));
            TimeSeriesInformation info = db.TimeSeriesInfo(key);
            Assert.Equal(now, info.FirstTimeStamp);
            Assert.Equal(now, info.LastTimeStap);
            Assert.Equal(retentionTime, info.RetentionTime);
        }

        [Fact]
        public void TestAddLabels()
        {
            IDatabase db = redisFixture.redis.GetDatabase();
            TimeStamp now = DateTime.Now;
            TimeSeriesLabel label = new TimeSeriesLabel("key", "value");
            var labels = new List<TimeSeriesLabel> { label };
            Assert.Equal(now, db.TimeSeriesAdd(key, now, 1.1, labels: labels));
            TimeSeriesInformation info = db.TimeSeriesInfo(key);
            Assert.Equal(now, info.FirstTimeStamp);
            Assert.Equal(now, info.LastTimeStap);
            Assert.Equal(labels, info.Labels);
        }


        [Fact]
        public void TestOldAdd()
        {
            TimeStamp old_dt = DateTime.Now;
            Thread.Sleep(1000);
            TimeStamp new_dt = DateTime.Now;
            IDatabase db = redisFixture.redis.GetDatabase();
            db.TimeSeriesCreate(key);
            db.TimeSeriesAdd(key, new_dt, 1.1);
            var ex = Assert.Throws<RedisServerException>(() => db.TimeSeriesAdd(key, old_dt, 1.2));
            Assert.Equal("TSDB: Timestamp cannot be older than the latest timestamp in the time series", ex.Message);
        }
    }
}
