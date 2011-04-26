using System;
using Bottles.Deployment.Writing;
using NUnit.Framework;

namespace Bottles.Tests.Deployment.Parsing
{
    public abstract class WritingReadingIntegratedTester
    {
        [SetUp]
        public void SetUp()
        {
            var writer = new ProfileWriter("profile1");
            defineTheProfile(writer);
            writer.Flush(FlushOptions.Wipeout);
        }

        protected abstract void defineTheProfile(ProfileWriter writer);


    }
}