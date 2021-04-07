using System;
using System.Threading.Tasks;

namespace AiTest
{
    public interface ISampleResultList
    {
        Task Add(SampleResult sampleResult);
       
    }
}