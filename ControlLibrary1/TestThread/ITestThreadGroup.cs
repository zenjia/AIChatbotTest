using System.Net.Http;
using System.Threading.Tasks;

namespace AiTest
{
    public interface ITestThreadGroup: ITestThreadGroupBase
    {
        Task AddSampleResult(SampleResult sampleResult);
      
        HttpClient GetHttpClient();
        int GenerateSampleId();
        int GetActiveThreadCount();
       
        int GetChatRoundCount();
        int GetChatDelay();

        int GetReadTimeOut();

    }
}