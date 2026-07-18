using FluentAssertions;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.SpacedRepetition;

public sealed class Sm2SchedulerTests
{
    private static readonly DateOnly Today = new(2026, 7, 18);

    private static Sm2CardState NewCard() => new(Sm2Scheduler.InitialEaseFactor, 0, 0);

    [Fact]
    public void Schedule_GoodGoodGood_IntervalsGrow1Then6ThenAboutFifteenAndEaseFactorIsUnchanged()
    {
        // "Good" is SM-2 quality 4: 0.1 - (5-4)*(0.08+(5-4)*0.02) = 0.1 - 0.10 = 0, so a Good streak
        // leaves the ease factor untouched at its initial 2.5 - only the interval/repetitions move.
        var state = NewCard();

        var first = Sm2Scheduler.Schedule(state, quality: 4, Today);
        first.Repetitions.Should().Be(1);
        first.IntervalDays.Should().Be(1);
        first.EaseFactor.Should().BeApproximately(2.5, 0.0001);
        first.NextReviewDateUtc.Should().Be(Today.AddDays(1));

        var second = Sm2Scheduler.Schedule(new Sm2CardState(first.EaseFactor, first.IntervalDays, first.Repetitions), quality: 4, Today);
        second.Repetitions.Should().Be(2);
        second.IntervalDays.Should().Be(6);
        second.EaseFactor.Should().BeApproximately(2.5, 0.0001);

        var third = Sm2Scheduler.Schedule(new Sm2CardState(second.EaseFactor, second.IntervalDays, second.Repetitions), quality: 4, Today);
        third.Repetitions.Should().Be(3);
        // round(6 x 2.5) = 15.
        third.IntervalDays.Should().Be(15);
        third.EaseFactor.Should().BeApproximately(2.5, 0.0001);
    }

    [Fact]
    public void Schedule_EasyEveryTime_EaseFactorIncreasesByOneTenthEachReview()
    {
        // "Easy" is SM-2 quality 5: 0.1 - (5-5)*(...) = 0.1 flat increase every time.
        var state = NewCard();

        var first = Sm2Scheduler.Schedule(state, quality: 5, Today);
        first.EaseFactor.Should().BeApproximately(2.6, 0.0001);
        first.IntervalDays.Should().Be(1);

        var second = Sm2Scheduler.Schedule(new Sm2CardState(first.EaseFactor, first.IntervalDays, first.Repetitions), quality: 5, Today);
        second.EaseFactor.Should().BeApproximately(2.7, 0.0001);
        second.IntervalDays.Should().Be(6);

        var third = Sm2Scheduler.Schedule(new Sm2CardState(second.EaseFactor, second.IntervalDays, second.Repetitions), quality: 5, Today);
        third.EaseFactor.Should().BeApproximately(2.8, 0.0001);
        // round(6 x 2.7) = round(16.2) = 16.
        third.IntervalDays.Should().Be(16);
    }

    [Fact]
    public void Schedule_HardEveryTime_EaseFactorDecreasesByFourteenHundredthsEachReviewButStillProgressesTheInterval()
    {
        // "Hard" is SM-2 quality 3: 0.1 - (5-3)*(0.08+(5-3)*0.02) = 0.1 - 2*0.12 = -0.14 per review.
        // Still a "correct" response (q >= 3), so the interval keeps progressing normally.
        var state = NewCard();

        var first = Sm2Scheduler.Schedule(state, quality: 3, Today);
        first.Repetitions.Should().Be(1);
        first.IntervalDays.Should().Be(1);
        first.EaseFactor.Should().BeApproximately(2.36, 0.0001);

        var second = Sm2Scheduler.Schedule(new Sm2CardState(first.EaseFactor, first.IntervalDays, first.Repetitions), quality: 3, Today);
        second.Repetitions.Should().Be(2);
        second.IntervalDays.Should().Be(6);
        second.EaseFactor.Should().BeApproximately(2.22, 0.0001);
    }

    [Fact]
    public void Schedule_AgainOnABrandNewCard_ResetsToIntervalOneAndLowersEaseFactorButNotBelowThePreviousValueMinusThePenalty()
    {
        // "Again" is SM-2 quality 0: 0.1 - 5*(0.08+5*0.02) = 0.1 - 0.9 = -0.8 per review.
        var state = NewCard();

        var result = Sm2Scheduler.Schedule(state, quality: 0, Today);

        result.Repetitions.Should().Be(0);
        result.IntervalDays.Should().Be(1);
        result.EaseFactor.Should().BeApproximately(1.7, 0.0001);
        result.NextReviewDateUtc.Should().Be(Today.AddDays(1));
    }

    [Fact]
    public void Schedule_AGoodStreakFollowedByAnAgain_ResetsRepetitionsAndIntervalButKeepsEaseFactorAboveTheFloor()
    {
        // 3x Good leaves EaseFactor at 2.5 (see the GoodGoodGood test), repetitions at 3, interval at 15.
        var afterGood1 = Sm2Scheduler.Schedule(NewCard(), quality: 4, Today);
        var afterGood2 = Sm2Scheduler.Schedule(new Sm2CardState(afterGood1.EaseFactor, afterGood1.IntervalDays, afterGood1.Repetitions), quality: 4, Today);
        var afterGood3 = Sm2Scheduler.Schedule(new Sm2CardState(afterGood2.EaseFactor, afterGood2.IntervalDays, afterGood2.Repetitions), quality: 4, Today);
        afterGood3.Repetitions.Should().Be(3);
        afterGood3.IntervalDays.Should().Be(15);
        afterGood3.EaseFactor.Should().BeApproximately(2.5, 0.0001);

        var afterAgain = Sm2Scheduler.Schedule(
            new Sm2CardState(afterGood3.EaseFactor, afterGood3.IntervalDays, afterGood3.Repetitions), quality: 0, Today);

        afterAgain.Repetitions.Should().Be(0);
        afterAgain.IntervalDays.Should().Be(1);
        // 2.5 - 0.8 = 1.7 - well above the 1.3 floor, so it is NOT clamped.
        afterAgain.EaseFactor.Should().BeApproximately(1.7, 0.0001);
        afterAgain.EaseFactor.Should().BeGreaterThan(Sm2Scheduler.MinEaseFactor);
    }

    [Fact]
    public void Schedule_RepeatedAgainReviews_NeverLetsEaseFactorDropBelowTheDocumentedFloor()
    {
        var state = NewCard();

        // Review 1: 2.5 - 0.8 = 1.7 (above floor).
        var first = Sm2Scheduler.Schedule(state, quality: 0, Today);
        first.EaseFactor.Should().BeApproximately(1.7, 0.0001);

        // Review 2: 1.7 - 0.8 = 0.9, clamped up to the 1.3 floor.
        var second = Sm2Scheduler.Schedule(new Sm2CardState(first.EaseFactor, first.IntervalDays, first.Repetitions), quality: 0, Today);
        second.EaseFactor.Should().Be(Sm2Scheduler.MinEaseFactor);
        second.Repetitions.Should().Be(0);
        second.IntervalDays.Should().Be(1);

        // Review 3: 1.3 - 0.8 = 0.5, still clamped at the floor - it never goes negative or below 1.3.
        var third = Sm2Scheduler.Schedule(new Sm2CardState(second.EaseFactor, second.IntervalDays, second.Repetitions), quality: 0, Today);
        third.EaseFactor.Should().Be(Sm2Scheduler.MinEaseFactor);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public void Schedule_QualityOutsideZeroToFive_Throws(int invalidQuality)
    {
        var act = () => Sm2Scheduler.Schedule(NewCard(), invalidQuality, Today);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Schedule_NextReviewDateUtc_IsAlwaysTodayPlusTheNewIntervalDays()
    {
        var result = Sm2Scheduler.Schedule(new Sm2CardState(2.5, 6, 2), quality: 4, Today);

        result.NextReviewDateUtc.Should().Be(Today.AddDays(result.IntervalDays));
    }
}
