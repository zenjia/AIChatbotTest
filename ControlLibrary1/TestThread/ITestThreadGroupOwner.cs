namespace AiTest
{
    public interface ITestThreadGroupOwner
    {
        string TestPlanPath { get; }

        ILog GetLog();
    }
}