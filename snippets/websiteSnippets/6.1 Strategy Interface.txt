using Nest;

namespace website.DataLayer.InitStrategies
{
    public interface IInitStrategy
    {
        void Init(IElasticClient client, string indexName);
    }
}