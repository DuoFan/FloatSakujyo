using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class ParameterException : System.Exception
    {
        public override string Message => $"{parameter} is {adjective}";
        string parameter;
        string adjective;
        public ParameterException(string _parameter,string _adjective)
        {
            parameter = _parameter;
            adjective = _adjective;
        }
    }
}
