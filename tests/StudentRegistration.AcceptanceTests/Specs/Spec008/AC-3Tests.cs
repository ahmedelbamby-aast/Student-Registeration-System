using System.Reflection;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class AC_3Tests
{
    [Fact]
    public void Blocking_hold_added_before_locked_re_resolution_prevents_consumer_commit()
    {
        RequireAcademicsType(
            "StudentRegistration.Academics.Application.StudentAcademicProfileService");

        var studentTermState = new StudentTermStateDouble();
        var consumer = new RegistrationBoundaryTestConsumer(studentTermState);
        var commitCallbackInvoked = false;

        Assert.Equal("ELIGIBLE", studentTermState.ResolveRegistrationDecision());
        studentTermState.AddBlockingHold("REGISTRATION_HOLD");

        var result = consumer.TryCommit(
            () => commitCallbackInvoked = true);

        Assert.Equal("HOLD_BLOCKED", result);
        Assert.Equal(1, studentTermState.LockCount);
        Assert.True(studentTermState.WasResolvedWhileLocked);
        Assert.False(commitCallbackInvoked);
    }

    private static void RequireAcademicsType(string fullName)
    {
        var type = Assembly.Load("StudentRegistration.Academics").GetType(fullName);
        Assert.True(type is not null, $"The required Academics service {fullName} is missing.");
    }

    private sealed class RegistrationBoundaryTestConsumer(StudentTermStateDouble state)
    {
        public string TryCommit(Action commitCallback) =>
            state.LockAndResolve(decision =>
            {
                if (decision == "HOLD_BLOCKED")
                {
                    return decision;
                }

                commitCallback();
                return "COMMITTED";
            });
    }

    private sealed class StudentTermStateDouble
    {
        private bool _hasBlockingHold;
        private bool _isLocked;

        public int LockCount { get; private set; }
        public bool WasResolvedWhileLocked { get; private set; }

        public void AddBlockingHold(string code)
        {
            Assert.Equal("REGISTRATION_HOLD", code);
            _hasBlockingHold = true;
        }

        public string ResolveRegistrationDecision()
        {
            WasResolvedWhileLocked |= _isLocked;
            return _hasBlockingHold ? "HOLD_BLOCKED" : "ELIGIBLE";
        }

        public string LockAndResolve(Func<string, string> continuation)
        {
            Assert.False(_isLocked);
            _isLocked = true;
            LockCount++;
            try
            {
                return continuation(ResolveRegistrationDecision());
            }
            finally
            {
                _isLocked = false;
            }
        }
    }
}
