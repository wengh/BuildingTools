using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathos.Parser;
using UnityEngine;

namespace BuildingTools
{
    public class Calculator
    {
        public readonly MathParser parser = new MathParser();
        public readonly List<string> inputs = new List<string>();
        public readonly List<double> outputs = new List<double>();
        private readonly StringBuilder log = new StringBuilder();
        private readonly StringBuilder lineNumber = new StringBuilder();
        private int position = 0;
        private int cursor = -1;

        public string Log => log.ToString();
        public string LineNumber => lineNumber.ToString();

        public Calculator()
        {
            parser.LocalFunctions["clear"] = inputs =>
            {
                log.Clear();
                lineNumber.Clear();
                return double.NaN;
            };
            parser.LocalFunctions["help"] = inputs =>
            {
                AddLog("Functions:\n" + string.Join("\n", parser.LocalFunctions.Keys.OrderBy(x => x)));
                AddLog("Variables:\n" + string.Join("\n", parser.LocalVariables.Keys.OrderBy(x => x).Select((x) => $"{x} = {parser.LocalVariables[x]}")));
                return double.NaN;
            };
            parser.LocalFunctions["out"] = inputs => outputs[(int)Math.Round(inputs[0])];
        }

        public double Evaluate(string expression)
        {
            double output = 0;
            try
            {
                output = parser.ProgrammaticallyParse(expression);
                parser.LocalVariables["_"] = output;
                AddLog(expression, output.ToString());
                outputs.Add(output);
            }
            catch (Exception e)
            {
                AddLog(expression, e.ToString());
                outputs.Add(double.NaN);
            }
            inputs.Add(expression);
            cursor = -1;
            return output;
        }

        private void AddLog(string input, string output)
        {
            lineNumber.AppendLine($"In [{position}]:");
            log.AppendLine(input);
            lineNumber.AppendLine($"Out[{position++}]:");
            log.AppendLine(output);
            log.AppendLine();
            lineNumber.AppendLine(string.Concat(output.Where(x => x == '\n')));
        }

        private void AddLog(string output)
        {
            log.AppendLine(output);
            log.AppendLine();
            lineNumber.AppendLine(string.Concat(output.Where(x => x == '\n')));
            lineNumber.AppendLine();
        }

        public string GetPreviousInput() => inputs[inputs.Count - 1 - (cursor + 1 <= inputs.Count ? ++cursor : cursor)];

        public string GetNextInput()
        {
            if (cursor > 0)
                return inputs[inputs.Count - 1 - (--cursor)];
            else
            {
                cursor = -1;
                return "";
            }
        }
    }
}
