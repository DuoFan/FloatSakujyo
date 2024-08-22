
namespace GameExtension
{
    public interface IIDAmountInfo : IIDProvider, IAmountProvider
    {
        void Adapt(int id, int amount);
    }
}