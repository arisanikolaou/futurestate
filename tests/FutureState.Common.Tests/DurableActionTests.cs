using FutureState;
using System;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Common.Tests
{
    [Story]
    [Collection("DurableActionTests")]
    public class ShouldBeAbleToExecuteActionsDurablyStory
    {
        int currentExecutionAttempts = 0;
        bool keepThrowingException = false;
        int maxRetries = 3;
        private DurableAction _subject;

        void GivenADurableAction()
        {
            _subject = new DurableAction(() =>
            {
                currentExecutionAttempts++;

                if (currentExecutionAttempts == 1 || keepThrowingException)
                    throw new TimeoutException();
            });
        }

        void WhenExecutingWithAnErrorFirstTime()
        {
            _subject.Invoke(maxRetries, TimeSpan.FromMilliseconds(10));
        }

        void AndWhenExecutingPastMaximumRetries()
        {
            keepThrowingException = true;

            Assert.Throws<TimeoutException>(() => _subject.Invoke(maxRetries, TimeSpan.FromMilliseconds(10)));
        }

        void ThenActionShouldExecuteFirstTimeButNotAfterMaxRetries()
        {
            // 
        }


        [BddfyFact]
        public void ShouldBeAbleToExecuteActionsDurably()
        {
            this.BDDfy();
        }
    }
}
