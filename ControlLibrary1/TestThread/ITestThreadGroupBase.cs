using System.Threading.Tasks;

namespace AiTest
{
    public interface ITestThreadGroupBase
    {
        ILog Log { get; }

        Task DecActiveThreadCount();
    }
}