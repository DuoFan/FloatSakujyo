using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class ParameterEmptyException : ParameterException
    {
        public ParameterEmptyException(string parameter) : base(parameter, "Empty")
        {

        }
    }
}
