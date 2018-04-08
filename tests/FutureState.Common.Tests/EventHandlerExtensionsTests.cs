using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace FutureState.Common.Tests
{
    public class EventHandlerExtensionsTests
    {
        private event EventHandler<MyEventArgs> Event1;
        private event EventHandler Event2;
        private event EventHandler<MyEventArgs> Event3;
        private event EventHandler Event4;


        private void EventHandlerExtensionsTestsEvent1(object sender, EventArgs evtArgs)
        {
            var args = evtArgs as MyEventArgs;

            if (args.ThrowExceptionAfterHitCount == args.HitCount)
                throw new Exception($"Exception {args.HitCount}.");

            args.HitCount++;
        }

        private void EventHandlerExtensionsTestsEvent1(object sender, MyEventArgs args)
        {
            if (args.ThrowExceptionAfterHitCount == args.HitCount)
                throw new Exception($"Exception {args.HitCount}.");

            args.HitCount++;
        }

        private void AlwaysThrowException(object sender, EventArgs args)
        {
            throw new NotImplementedException();
        }

        public class MyEventArgs : EventArgs
        {
            public int ThrowExceptionAfterHitCount { get; set; }

            public int HitCount { get; set; }
        }

        [Fact]
        public void RaisesEventsToAllSubscribersSpecialized()
        {
            // arrange 
            Event1 += EventHandlerExtensionsTestsEvent1;
            Event1 += AlwaysThrowException;

            var args = new MyEventArgs {ThrowExceptionAfterHitCount = 1};
            var errors = new List<Exception>();

            // ReSharper disable once ConvertToLocalFunction
            Action<Exception> handler = exception =>
            {
                lock (this)
                {
                    errors.Add(exception);
                }
            };

            // act
            Event1.AsyncRaiseSafe(this, args, handler).Wait(); //1 pass 1 fail
            Event1.AsyncRaiseSafe(this, args, handler).Wait(); //2 fail
            Event1.AsyncRaiseSafe(this, args, handler).Wait(); //2 fail

            Thread.Sleep(500);

            Assert.Equal(5, errors.Count);
            Assert.Equal(1, args.HitCount);

            args.HitCount = 0;

            // asset
            Event1.AsyncRaiseSafe(this, args, handler).Wait(); //pass

            Assert.Equal(1, args.HitCount);
        }

        [Fact]
        public void RaisesEventsToAllSubscribersStandardDelegate()
        {
            // arrange
            Event2 += EventHandlerExtensionsTestsEvent1;
            Event2 += AlwaysThrowException;

            var args = new MyEventArgs {ThrowExceptionAfterHitCount = 1};
            var errors = new List<Exception>();

            // ReSharper disable once ConvertToLocalFunction
            Action<Exception> handler = exception => { errors.Add(exception); };

            // act
            Event2.AsyncRaiseSafe(this, args, handler).Wait(); //1 pass 1 fail
            Event2.AsyncRaiseSafe(this, args, handler).Wait(); //2 fail
            Event2.AsyncRaiseSafe(this, args, handler).Wait(); //2 fail

            Thread.Sleep(1500); //avoid race condition

            Assert.Equal(5, errors.Count);
            Assert.Equal(1, args.HitCount);

            args.HitCount = 0;

            Event2.AsyncRaiseSafe(this, args, handler).Wait(); //pass

            Assert.Equal(1, args.HitCount);
        }


        [Fact]
        public void RaisesEventsToAllSubscribersSync()
        {
            // arrange
            Event3 += EventHandlerExtensionsTestsEvent1;
            Event3 += AlwaysThrowException;

            var args = new MyEventArgs {ThrowExceptionAfterHitCount = 1};
            var errors = new List<Exception>();

            // ReSharper disable once ConvertToLocalFunction
            Action<Exception> handler = exception => { errors.Add(exception); };

            // act
            Event3.RaiseSafe(this, args, handler); //1 pass 1 fail
            Event3.RaiseSafe(this, args, handler); //2 fail
            Event3.RaiseSafe(this, args, handler); //2 fail

            Assert.Equal(5, errors.Count);
            Assert.Equal(1, args.HitCount);

            args.HitCount = 0;

            Event3.RaiseSafe(this, args, handler); //pass

            Assert.Equal(1, args.HitCount);
        }

        [Fact]
        public void RaisesEventsToAllSubscribersSyncSpecialized()
        {
            // arrange
            Event4 += EventHandlerExtensionsTestsEvent1;
            Event4 += AlwaysThrowException;

            var args = new MyEventArgs {ThrowExceptionAfterHitCount = 1};
            var errors = new List<Exception>();

            // ReSharper disable once ConvertToLocalFunction
            Action<Exception> handler = exception => { errors.Add(exception); };

            // act
            Event4.RaiseSafe(this, args, handler); //1 pass 1 fail
            Event4.RaiseSafe(this, args, handler); //2 fail
            Event4.RaiseSafe(this, args, handler); //2 fail

            Assert.Equal(5, errors.Count);
            Assert.Equal(1, args.HitCount);

            args.HitCount = 0;

            // act
            Event4.RaiseSafe(this, args, handler); //pass

            Assert.Equal(1, args.HitCount);
        }
    }
}