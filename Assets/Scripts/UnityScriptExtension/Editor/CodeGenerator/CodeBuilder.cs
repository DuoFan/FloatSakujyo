using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EditorExtension
{
    public class CodeBuilder : IBuild<string>
    {
        protected List<CodeBuilder> children;
        public void AddChildren(CodeBuilder child)
        {
            if (children == null)
            {
                children = new List<CodeBuilder>();
            }
            children.Add(child);
        }
        public IFCodeBuilder AddIF(string prediction, string code)
        {
            IFCodeBuilder ifCode = new IFCodeBuilder();
            ifCode.SetIf(prediction, code);
            AddChildren(ifCode);
            return ifCode;
        }
        public LineCodeBuilder AddLine(string code)
        {
            LineCodeBuilder lineCode = new LineCodeBuilder();
            lineCode.SetLine(code);
            AddChildren(lineCode);
            return lineCode;
        }
        public virtual string Build()
        {
            if (children != null)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < children.Count; i++)
                {
                    sb.Append($"\n{children[i].Build()}");
                }
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
    struct PredictionCode
    {
        public string prediction;
        public string code;
    }
    public class IFCodeBuilder : CodeBuilder
    {
        PredictionCode predictionCode;
        List<ElseIFCodeBuilder> elseIFCodes;
        ElseCodeBuilder elseCode;

        public void SetIf(string predition, string code)
        {
            predictionCode.prediction = predition;
            predictionCode.code = code;
        }
        public ElseIFCodeBuilder AddElseIf(string predition, string code)
        {
            if (elseIFCodes == null)
            {
                elseIFCodes = new List<ElseIFCodeBuilder>();
            }
            var elseIfCode = new ElseIFCodeBuilder();
            elseIfCode.SetElseIf(predition, code);
            elseIFCodes.Add(elseIfCode);
            return elseIfCode;
        }
        public ElseCodeBuilder SetElse(string code)
        {
            if (elseCode == null)
            {
                elseCode = new ElseCodeBuilder();
            }
            elseCode.SetElse(code);
            return elseCode;
        }
        public override string Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"\nif({predictionCode.prediction})");
            sb.Append($"\n{{");
            sb.Append(predictionCode.code);
            sb.Append(base.Build());
            sb.Append($"\n}}");
            if (elseIFCodes != null)
            {
                for (int i = 0; i < elseIFCodes.Count; i++)
                {
                    sb.Append(elseIFCodes[i].Build());
                }
            }
            if (elseCode != null)
            {
                sb.Append(elseCode.Build());
            }
            return sb.ToString();
        }
    }
    public class ElseIFCodeBuilder : CodeBuilder
    {
        PredictionCode predictionCode;
        public void SetElseIf(string predition, string code)
        {
            predictionCode.prediction = predition;
            predictionCode.code = code;
        }

        public override string Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"\nelse if({predictionCode.prediction})");
            sb.Append($"\n{{");
            sb.Append(predictionCode.code);
            sb.Append(base.Build());
            sb.Append($"\n}}");
            return sb.ToString();
        }
    }
    public class ElseCodeBuilder : CodeBuilder
    {
        string code;

        public void SetElse(string _code)
        {
            code = _code;
        }

        public override string Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"\nelse");
            sb.Append($"\n{{");
            sb.Append(code);
            sb.Append(base.Build());
            sb.Append($"\n}}");
            return sb.ToString();
        }
    }
    public class LineCodeBuilder : CodeBuilder
    {
        string lineCode = string.Empty;
        public void SetLine(string code)
        {
            lineCode = code;
        }
        public override string Build()
        {
            if (children != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"{lineCode};");
                for (int i = 0; i < children.Count; i++)
                {
                    sb.Append($"\n{children[i].Build()}");
                }
                return sb.ToString();
            }
            else
            {
                return $"{lineCode};";
            }
        }
    }
}
