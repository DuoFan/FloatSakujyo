using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public interface IStorePayment
    {
        bool CheckPay(IStoreItem item);
        void OnSuccess(IStoreItem item);
        void OnFail(IStoreItem item);
    }
}
