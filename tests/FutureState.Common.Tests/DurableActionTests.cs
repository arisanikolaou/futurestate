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
        private readonly int maxRetries = 3;
        private DurableAction _subject;
        private int currentExecutionAttempts;
        private bool keepThrowingException;

        private void GivenADurableAction()
        {
            _subject = new DurableAction(() =>
            {
                currentExecutionAttempts++;

                if (currentExecutionAttempts == 1 || keepThrowingException)
                    throw new TimeoutException();
            });
        }

        private void WhenExecutingWithAnErrorFirstTime()
        {
            _subject.Invoke(maxRetries, TimeSpan.FromMilliseconds(10));
        }

        private void AndWhenExecutingPastMaximumRetries()
        {
            keepThrowingException = true;

            Assert.Throws<TimeoutException>(() => _subject.Invoke(maxRetries, TimeSpan.FromMilliseconds(10)));
        }

        private void ThenActionShouldExecuteFirstTimeButNotAfterMaxRetries()
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