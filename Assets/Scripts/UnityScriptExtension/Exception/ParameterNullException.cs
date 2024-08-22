using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class ParameterNullException : ParameterException
    {
        public ParameterNullException(string _parameter) : base(_parameter, "Null")
        {

        }
    }
}
