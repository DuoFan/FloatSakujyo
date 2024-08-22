using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension
{
    public static class Store
    {
        public static bool TryBuyItem(IStoreItem item, IStorePayment payment)
        {
            bool result = payment.CheckPay(item);
            if (result)
            {
                payment.OnSuccess(item);
            }
            else
            {
                payment.OnFail(item);
            }
            return result;
        }
    }
}
