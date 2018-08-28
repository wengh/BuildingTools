﻿/* 
 * Copyright (C) 2012-2018, Mathos Project.
 * All rights reserved.
 * 
 * Please see the license file in the project folder,
 * or go to https://github.com/MathosProject/Mathos-Parser/blob/master/LICENSE.md.
 * 
 * Please feel free to ask me directly at my email!
 *  artem@artemlos.net
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Mathos.Parser
{
    /// <summary>
    /// This is a mathematical expression parser that allows you to perform calculations on string values.
    /// </summary>
    public class MathParser
    {
        private const double Deg2Rad = Math.PI / 180;
        private const double Rad2Deg = 180 / Math.PI;

        private const double Ms2Kmh = 3.6;
        private const double Kmh2Ms = 1 / Ms2Kmh;

        private const double Kn2Kmh = 1.852;
        private const double Kmh2Kn = 1 / Kn2Kmh;

        private const double Kn2Ms = Kn2Kmh * Kmh2Ms;
        private const double Ms2Kn = 1 / Kn2Ms;

        private const double In2Mm = 25.4;
        private const double Mm2In = 1 / In2Mm;

        #region Properties

        /// <summary>
        /// All operators that you want to define should be inside this property.
        /// </summary>
        public Dictionary<string, Func<double, double, double>> Operators { get; set; }

        /// <summary>
        /// All functions that you want to define should be inside this property.
        /// </summary>
        public Dictionary<string, Func<double[], double>> LocalFunctions { get; set; }

        /// <summary>
        /// All variables that you want to define should be inside this property.
        /// </summary>
        public Dictionary<string, double> LocalVariables { get; set; }

        /// <summary>
        /// When converting the result from the Parse method or ProgrammaticallyParse method ToString(),
        /// please use this culture info.
        /// </summary>
        public CultureInfo CultureInfo { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the MathParser class, and optionally with
        /// predefined functions, operators, and variables.
        /// </summary>
        /// <param name="loadPreDefinedFunctions">This will load abs, cos, cosh, arccos, sin, sinh, arcsin, tan, tanh, arctan, sqrt, rem, and round.</param>
        /// <param name="loadPreDefinedOperators">This will load %, *, :, /, +, -, >, &lt;, and =</param>
        /// <param name="loadPreDefinedVariables">This will load pi, tao, e, phi, major, minor, pitograd, and piofgrad.</param>
        /// <param name="cultureInfo">The culture info to use when parsing. If null, defaults to invariant culture.</param>
        public MathParser(bool loadPreDefinedFunctions = true, bool loadPreDefinedOperators = true, bool loadPreDefinedVariables = true, CultureInfo cultureInfo = null)
        {
            if (loadPreDefinedOperators)
            {
                Operators = new Dictionary<string, Func<double, double, double>>(10)
                {
                    ["^"] = Math.Pow,
                    ["%"] = (a, b) => a % b,
                    [":"] = (a, b) => a / b,
                    ["/"] = (a, b) => a / b,
                    ["*"] = (a, b) => a * b,
                    ["-"] = (a, b) => a - b,
                    ["+"] = (a, b) => a + b,

                    [">"] = (a, b) => a > b ? 1 : 0,
                    ["<"] = (a, b) => a < b ? 1 : 0,
                    ["="] = (a, b) => Math.Abs(a - b) < 0.00000001 ? 1 : 0
                };
            }
            else
                Operators = new Dictionary<string, Func<double, double, double>>();

            if (loadPreDefinedFunctions)
            {
                LocalFunctions = new Dictionary<string, Func<double[], double>>(26)
                {
                    ["abs"] = inputs => Math.Abs(inputs[0]),

                    ["cos"] = inputs => Math.Cos(inputs[0] * Deg2Rad),
                    ["cosh"] = inputs => Math.Cosh(inputs[0] * Deg2Rad),
                    ["acos"] = inputs => Math.Acos(inputs[0]) * Rad2Deg,
                    ["arccos"] = inputs => Math.Acos(inputs[0]) * Rad2Deg,

                    ["sin"] = inputs => Math.Sin(inputs[0] * Deg2Rad),
                    ["sinh"] = inputs => Math.Sinh(inputs[0] * Deg2Rad),
                    ["asin"] = inputs => Math.Asin(inputs[0]) * Rad2Deg,
                    ["arcsin"] = inputs => Math.Asin(inputs[0]) * Rad2Deg,

                    ["tan"] = inputs => Math.Tan(inputs[0] * Deg2Rad),
                    ["tanh"] = inputs => Math.Tanh(inputs[0] * Deg2Rad),
                    ["atan"] = inputs => Math.Atan(inputs[0]) * Rad2Deg,
                    ["arctan"] = inputs => Math.Atan(inputs[0]) * Rad2Deg,

                    ["sqrt"] = inputs => Math.Sqrt(inputs[0]),
                    ["pow"] = inputs => Math.Pow(inputs[0], inputs[1]),
                    ["root"] = inputs => Math.Pow(inputs[0], 1 / inputs[1]),
                    ["rem"] = inputs => Math.IEEERemainder(inputs[0], inputs[1]),

                    ["sign"] = inputs => Math.Sign(inputs[0]),
                    ["exp"] = inputs => Math.Exp(inputs[0]),

                    ["floor"] = inputs => Math.Floor(inputs[0]),
                    ["ceil"] = inputs => Math.Ceiling(inputs[0]),
                    ["ceiling"] = inputs => Math.Ceiling(inputs[0]),
                    ["round"] = inputs => Math.Round(inputs[0]),
                    ["truncate"] = inputs => inputs[0] < 0 ? -Math.Floor(-inputs[0]) : Math.Floor(inputs[0]),

                    ["log"] = inputs =>
                    {
                        switch (inputs.Length)
                        {
                            case 1:
                                return Math.Log10(inputs[0]);
                            case 2:
                                return Math.Log(inputs[0], inputs[1]);
                            default:
                                return 0;
                        }
                    },

                    ["ln"] = inputs => Math.Log(inputs[0])
                };
            }
            else
                LocalFunctions = new Dictionary<string, Func<double[], double>>();

            if (loadPreDefinedVariables)
            {
                LocalVariables = new Dictionary<string, double>(8)
                {
                    ["pi"] = Math.PI,
                    ["tau"] = 2 * Math.PI,

                    ["e"] = Math.E,
                    ["phi"] = 1.61803398874989,
                    ["major"] = 0.61803398874989,
                    ["minor"] = 0.38196601125011,

                    ["rad2deg"] = Rad2Deg,
                    ["deg2rad"] = Deg2Rad,

                    ["ms2kmh"] = Ms2Kmh,
                    ["kmh2ms"] = Kmh2Ms,

                    ["kn2kmh"] = Kn2Kmh,
                    ["kmh2kn"] = Kmh2Kn,

                    ["kn2ms"] = Kn2Ms,
                    ["ms2kn"] = Ms2Kn,

                    ["in2mm"] = In2Mm,
                    ["mm2in"] = Mm2In
                };
            }
            else
                LocalVariables = new Dictionary<string, double>();

            CultureInfo = cultureInfo ?? CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Enter the math expression in form of a string.
        /// </summary>
        /// <param name="mathExpression">The math expression to parse.</param>
        /// <returns>The result of executing <paramref name="mathExpression"/>.</returns>
        public double Parse(string mathExpression) => MathParserLogic(Lexer(mathExpression));

        /// <summary>
        /// Enter the math expression in form of a list of tokens.
        /// </summary>
        /// <param name="mathExpression">The math expression to parse.</param>
        /// <returns>The result of executing <paramref name="mathExpression"/>.</returns>
        public double Parse(ReadOnlyCollection<string> mathExpression) => MathParserLogic(new List<string>(mathExpression));

        /// <summary>
        /// Enter the math expression in form of a string. You might also add/edit variables using "let" keyword.
        /// For example, "let sampleVariable = 2+2".
        /// 
        /// Another way of adding/editing a variable is to type "varName := 20"
        /// 
        /// Last way of adding/editing a variable is to type "let varName be 20"
        /// </summary>
        /// <param name="mathExpression">The math expression to parse.</param>
        /// <param name="correctExpression">If true, correct <paramref name="correctExpression"/> of any typos.</param>
        /// <param name="identifyComments">If true, treat "#", "#{", and "}#" as comments.</param>
        /// <returns>The result of executing <paramref name="mathExpression"/>.</returns>
        public double ProgrammaticallyParse(string mathExpression, bool correctExpression = true, bool identifyComments = true)
        {
            if (identifyComments)
            {
                // Delete Comments #{Comment}#
                mathExpression = System.Text.RegularExpressions.Regex.Replace(mathExpression, "#\\{.*?\\}#", "");

                // Delete Comments #Comment
                mathExpression = System.Text.RegularExpressions.Regex.Replace(mathExpression, "#.*$", "");
            }

            if (correctExpression)
            {
                // this refers to the Correction function which will correct stuff like artn to arctan, etc.
                mathExpression = Correction(mathExpression);
            }

            string varName;
            double varValue;

            if (mathExpression.Contains("let"))
            {
                if (mathExpression.Contains("be"))
                {
                    varName = mathExpression.Substring(mathExpression.IndexOf("let", StringComparison.Ordinal) + 3,
                        mathExpression.IndexOf("be", StringComparison.Ordinal) -
                        mathExpression.IndexOf("let", StringComparison.Ordinal) - 3);
                    mathExpression = mathExpression.Replace(varName + "be", "");
                }
                else
                {
                    varName = mathExpression.Substring(mathExpression.IndexOf("let", StringComparison.Ordinal) + 3,
                        mathExpression.IndexOf("=", StringComparison.Ordinal) -
                        mathExpression.IndexOf("let", StringComparison.Ordinal) - 3);
                    mathExpression = mathExpression.Replace(varName + "=", "");
                }

                varName = varName.Replace(" ", "");
                mathExpression = mathExpression.Replace("let", "");

                varValue = Parse(mathExpression);

                if (LocalVariables.ContainsKey(varName))
                    LocalVariables[varName] = varValue;
                else
                    LocalVariables.Add(varName, varValue);

                return varValue;
            }

            if (!mathExpression.Contains(":="))
                return Parse(mathExpression);

            //mathExpression = mathExpression.Replace(" ", ""); // remove white space
            varName = mathExpression.Substring(0, mathExpression.IndexOf(":=", StringComparison.Ordinal));
            mathExpression = mathExpression.Replace(varName + ":=", "");

            varValue = Parse(mathExpression);
            varName = varName.Replace(" ", "");

            if (LocalVariables.ContainsKey(varName))
                LocalVariables[varName] = varValue;
            else
                LocalVariables.Add(varName, varValue);

            return varValue;
        }

        /// <summary>
        /// This will convert a string expression into a list of tokens that can be later executed by Parse or ProgrammaticallyParse methods.
        /// </summary>
        /// <param name="mathExpression">The math expression to tokenize.</param>
        /// <returns>The resulting tokens of <paramref name="mathExpression"/>.</returns>
        public ReadOnlyCollection<string> GetTokens(string mathExpression) => Lexer(mathExpression).AsReadOnly();

        #region Core

        /// <summary>
        /// This will correct sqrt() and arctan() written in different ways only.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string Correction(string input)
        {
            // Word corrections

            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(sqr|sqrt)\\b", "sqrt",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(atan2|arctan2)\\b", "arctan2",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //... and more

            return input;
        }

        /// <summary>
        /// Tokenizes <paramref name="expr"/>.
        /// </summary>
        /// <param name="expr">The expression to tokenize.</param>
        /// <returns>The tokens.</returns>
        private List<string> Lexer(string expr)
        {
            var token = "";
            var tokens = new List<string>();

            expr = expr.Replace("+-", "-");
            expr = expr.Replace("-+", "-");
            expr = expr.Replace("--", "+");

            for (var i = 0; i < expr.Length; i++)
            {
                var ch = expr[i];

                if (char.IsWhiteSpace(ch))
                    continue;

                if (char.IsLetter(ch))
                {
                    if (i != 0 && (char.IsDigit(expr[i - 1]) || expr[i - 1] == ')'))
                        tokens.Add("*");

                    token += ch;

                    while (i + 1 < expr.Length && char.IsLetterOrDigit(expr[i + 1]))
                        token += expr[++i];

                    tokens.Add(token);
                    token = "";

                    continue;
                }

                if (char.IsDigit(ch))
                {
                    token += ch;

                    while (i + 1 < expr.Length && (char.IsDigit(expr[i + 1]) || expr[i + 1] == '.'))
                        token += expr[++i];

                    tokens.Add(token);
                    token = "";

                    continue;
                }

                if (ch == '.')
                {
                    token += ch;

                    while (i + 1 < expr.Length && char.IsDigit(expr[i + 1]))
                        token += expr[++i];

                    tokens.Add(token);
                    token = "";

                    continue;
                }

                if (i + 1 < expr.Length && (ch == '-' || ch == '+') && char.IsDigit(expr[i + 1]) &&
                    (i == 0 || Operators.ContainsKey(expr[i - 1].ToString(
#if !NETSTANDARD1_4 
                        CultureInfo
#endif
                        )) ||
                     i - 1 > 0 && expr[i - 1] == '('))
                {
                    // if the above is true, then the token for that negative number will be "-1", not "-","1".
                    // to sum up, the above will be true if the minus sign is in front of the number, but
                    // at the beginning, for example, -1+2, or, when it is inside the brakets (-1).
                    // NOTE: this works for + as well!

                    token += ch;

                    while (i + 1 < expr.Length && (char.IsDigit(expr[i + 1]) || expr[i + 1] == '.'))
                        token += expr[++i];

                    tokens.Add(token);
                    token = "";

                    continue;
                }

                if (ch == '(')
                {
                    if (i != 0 && (char.IsDigit(expr[i - 1]) || char.IsDigit(expr[i - 1]) || expr[i - 1] == ')'))
                    {
                        tokens.Add("*");
                        tokens.Add("(");
                    }
                    else
                        tokens.Add("(");
                }
                else
                    tokens.Add(ch.ToString());
            }

            return tokens;
        }

        private double MathParserLogic(List<string> tokens)
        {
            // Variables replacement
            for (var i = 0; i < tokens.Count; i++)
            {
                if (LocalVariables.Keys.Contains(tokens[i]))
                    tokens[i] = LocalVariables[tokens[i]].ToString(CultureInfo);
            }

            while (tokens.IndexOf("(") != -1)
            {
                // getting data between "(" and ")"
                var open = tokens.LastIndexOf("(");
                var close = tokens.IndexOf(")", open); // in case open is -1, i.e. no "(" // , open == 0 ? 0 : open - 1

                if (open >= close)
                    throw new ArithmeticException("No closing bracket/parenthesis. Token: " + open.ToString(CultureInfo));

                var roughExpr = new List<string>();

                for (var i = open + 1; i < close; i++)
                    roughExpr.Add(tokens[i]);

                double tmpResult;

                var args = new List<double>();
                var functionName = tokens[open == 0 ? 0 : open - 1];

                if (LocalFunctions.Keys.Contains(functionName))
                {
                    if (roughExpr.Contains(","))
                    {
                        // converting all arguments into a decimal array
                        for (var i = 0; i < roughExpr.Count; i++)
                        {
                            var defaultExpr = new List<string>();
                            var firstCommaOrEndOfExpression =
                                roughExpr.IndexOf(",", i) != -1
                                    ? roughExpr.IndexOf(",", i)
                                    : roughExpr.Count;

                            while (i < firstCommaOrEndOfExpression)
                                defaultExpr.Add(roughExpr[i++]);

                            args.Add(defaultExpr.Count == 0 ? 0 : BasicArithmeticalExpression(defaultExpr));
                        }

                        // finally, passing the arguments to the given function
                        tmpResult = double.Parse(LocalFunctions[functionName](args.ToArray()).ToString(CultureInfo), CultureInfo);
                    }
                    else
                    {
                        // but if we only have one argument, then we pass it directly to the function
                        tmpResult = double.Parse(LocalFunctions[functionName](new[]
                        {
                            BasicArithmeticalExpression(roughExpr)
                        }).ToString(CultureInfo), CultureInfo);
                    }
                }
                else
                {
                    // if no function is need to execute following expression, pass it
                    // to the "BasicArithmeticalExpression" method.
                    tmpResult = BasicArithmeticalExpression(roughExpr);
                }

                // when all the calculations have been done
                // we replace the "opening bracket with the result"
                // and removing the rest.
                tokens[open] = tmpResult.ToString(CultureInfo);
                tokens.RemoveRange(open + 1, close - open);

                if (LocalFunctions.Keys.Contains(functionName))
                {
                    // if we also executed a function, removing
                    // the function name as well.
                    tokens.RemoveAt(open - 1);
                }
            }

            // at this point, we should have replaced all brackets
            // with the appropriate values, so we can simply
            // calculate the expression. it's not so complex
            // any more!
            return BasicArithmeticalExpression(tokens);
        }

        private double BasicArithmeticalExpression(List<string> tokens)
        {
            // PERFORMING A BASIC ARITHMETICAL EXPRESSION CALCULATION
            // THIS METHOD CAN ONLY OPERATE WITH NUMBERS AND OPERATORS
            // AND WILL NOT UNDERSTAND ANYTHING BEYOND THAT.

            switch (tokens.Count)
            {
                case 1:
                    return double.Parse(tokens[0], CultureInfo);
                case 2:
                    var op = tokens[0];

                    if (op == "-" || op == "+")
                    {
                        var first = op == "+" ? "" : (tokens[1].Substring(0, 1) == "-" ? "" : "-");

                        return double.Parse(first + tokens[1], CultureInfo);
                    }

                    return Operators[op](0, double.Parse(tokens[1], CultureInfo));
                case 0:
                    return 0;
            }

            foreach (var op in Operators)
            {
                int opPlace;

                while ((opPlace = tokens.IndexOf(op.Key)) != -1)
                {
                    var numberA = double.Parse(tokens[opPlace - 1], CultureInfo);
                    var numberB = double.Parse(tokens[opPlace + 1], CultureInfo);

                    var result = op.Value(numberA, numberB);

                    tokens[opPlace - 1] = result.ToString(CultureInfo);
                    tokens.RemoveRange(opPlace, 2);
                }
            }

            return double.Parse(tokens[0], CultureInfo);
        }

        #endregion
    }
}
