using BOA.Base;
using BOA.Proxy.Kernel.Card;

namespace ApiInspector.Invoking
{
    class ServiceWrapper
    {
        public static object Wrap(ObjectHelper objectHelper, BOA.Card.Contracts.CreditCard.Limit.GetCardAvailableLimitRequest request)
        {
            return CardLocalProxy.Call(objectHelper, (BOA.Card.Contracts.CreditCard.Limit.ICRDLimitService service) => service.GetCardAvailableLimit(request));
        }
    }
}