using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class ParameterNullOrEmptyException : ParameterException
    {
        public ParameterNullOrEmptyException(string _parameter) : base(_parameter, "Null or Empty")
        {

        }
    }
}
