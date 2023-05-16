using System;
using System.Collections.Generic;
using System.Linq;

namespace CbmCode.CodeGeneration
{
    public class Generate
    {
        readonly string[] _sourceLines;
        public Generate(string[] sourceLines)
        {
            _sourceLines = sourceLines;
        }

        public (bool success, List<List<string>> codeGenerations) Do()
        {
            List<string> cleanedLines = RemoveComments(_sourceLines);
            var (success, withSubstitutedVariables) = SubstituteVariables(cleanedLines);
            if (success)
            {
                List<string> withoutCompoundAssignment = SubstituteCompoundAssignment(withSubstitutedVariables);
                List<string> withInlinedConstants = InlineConstants(withoutCompoundAssignment);
                var withSubstitutedLabels = SubstituteLabelsLineNumbers(withInlinedConstants);
                return (true, L(_sourceLines.ToList(),
                    cleanedLines,
                    withSubstitutedVariables,
                    withoutCompoundAssignment,
                    withInlinedConstants,
                    withSubstitutedLabels));
            }
            else
                return (false, null);

            List<T> L<T>(params T[] args) => args.ToList();
        }

        List<string> RemoveComments(string[] lines)
        {
            //remove comments and blank lines:
            var cleanedLines = new List<string>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var l = line.Trim();
                if (l.StartsWith(@"//"))
                    continue;
                var index = l.IndexOf(@"//");
                if (index > -1)
                    l = l.Substring(0, index);
                l = l.Trim();
                cleanedLines.Add(l);
            }
            return cleanedLines;
        }

        (bool success, List<string> lines) SubstituteVariables(List<string> lines)
        {
            //collect declared variables and make sure there aren't too many of them:
            //Ponder: add a directive to remove variables when we no longer need them?

            var possibleFloatVariables = GenerateVariableNames();
            var availableFloatVariables = new Stack<string>(possibleFloatVariables);
            var actualFloatVariables = new Dictionary<string, string>();
            var floatCount = 0;

            var possibleIntVariables = possibleFloatVariables.Select(s => s + '%').ToArray();
            var availableIntVariables = new Stack<string>(possibleIntVariables);
            var actualIntVariables = new Dictionary<string, string>();
            var intCount = 0;

            var possibleStringVariables = possibleFloatVariables.Select(s => s + '$').ToArray();
            var availableStringVariables = new Stack<string>(possibleStringVariables);
            var actualStringVariables = new Dictionary<string, string>();
            var stringCount = 0;

            var sansVariableDeclarations = new List<string>();
            foreach (var line in lines)
            {
                if (line.StartsWith("@variables:", StringComparison.InvariantCultureIgnoreCase))
                {
                    var declaredVariables = line.Substring(11).Split(',');
                    foreach (var variable in declaredVariables)
                    {
                        var v = variable.Trim();
                        if (v.EndsWith("%"))//int variables
                        {
                            actualIntVariables.Add(v, availableIntVariables.Pop());
                            if (intCount >= possibleIntVariables.Length)
                                return (false, null);
                        }
                        else if (v.EndsWith("$"))//string variables
                        {
                            actualStringVariables.Add(v, availableStringVariables.Pop());
                            if (stringCount >= possibleStringVariables.Length)
                                return (false, null);
                        }
                        else//float variables
                        {
                            actualFloatVariables.Add(v, availableFloatVariables.Pop());
                            if (floatCount >= possibleFloatVariables.Length)
                                return (false, null);
                        }
                    }
                }
                else
                    sansVariableDeclarations.Add(line);
            }

            //replace all occurences of given variables with the generated ones:

            Dictionary<string, string> actualVariables = A(actualFloatVariables, actualIntVariables, actualStringVariables)
                .SelectMany(dict => dict)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            var withVariableSubstitution = new List<string>();
            foreach (var line in sansVariableDeclarations)
            {
                var newLine = line;
                foreach (var av in actualVariables)
                    newLine = newLine.Replace(av.Key, av.Value);
                withVariableSubstitution.Add(newLine);
            }

            return (true, withVariableSubstitution);

            T[] A<T>(params T[] args) => args;
        }

        private string[] GenerateVariableNames()
        {
            var result = new List<string>();

            const string firstCharacter = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string secondCharacter = firstCharacter + "0123456789";

            foreach (var c in firstCharacter)
                result.Add(c.ToString());

            foreach (var c in firstCharacter)
                foreach (var c2 in secondCharacter)
                    result.Add($"{c}{c2}");

            result.Reverse();
            return result.ToArray();
        }

        List<string> SubstituteCompoundAssignment(List<string> lines)
        {
            /* This:
             * x += 42
             * Becomes:
             * x = x + 42
             */
            var result = new List<string>();

            foreach (var line in lines)
            {
                var indices = new List<int>();
                var index = line.IndexOf('=');
                while (index >= 0)
                {
                    indices.Add(index);
                    if (index < line.Length)
                        index = line.IndexOf('=', index + 1);
                    else
                        break;
                }

                if (indices.Count > 0 && indices[0] == 0)
                    indices = indices.GetRange(1, indices.Count - 1);

                var substitutionPairs = new List<(string oldExpression, string newExpression)>();
                foreach (var assignmentIndex in indices)
                {
                    int beforeEqualSign = assignmentIndex - 1;
                    var variable = string.Empty;
                    var op = string.Empty;
                    var compoundAssignment = "=";
                    var state = 0;
                    if (!char.IsWhiteSpace(line[beforeEqualSign]))
                    {//It's a compound assignment!
                        for (int i = beforeEqualSign; i >= 0; i--)
                        {
                            var cc = line[i];
                            compoundAssignment += cc;
                            if (state == 0)//operator state
                            {
                                if (char.IsWhiteSpace(cc))
                                    state++;
                                else
                                    op += cc;
                            }
                            else if (state == 1)//intermediare whitespace state
                            {
                                if (!char.IsWhiteSpace(cc))
                                {
                                    variable += cc;
                                    state++;
                                }
                                else
                                    continue;
                            }
                            else//variable state
                            {
                                if (char.IsWhiteSpace(cc))
                                    break;
                                variable += cc;
                            }
                        }
                        variable = Reverse(variable);
                        op = Reverse(op);
                        compoundAssignment = Reverse(compoundAssignment.Trim());
                        var newExpression = $"{variable} = {variable} {op}";
                        substitutionPairs.Add((compoundAssignment, newExpression));
                    }
                }
                substitutionPairs.Reverse();
                var newLine = line;
                foreach (var (oldExpr, newExpr) in substitutionPairs)
                    newLine = newLine.Replace(oldExpr, newExpr);
                result.Add(newLine);
            }

            return result;
        }

        private static string Reverse(string value)
        {
            var charArray = value.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        List<string> InlineConstants(List<string> lines)
        {

            var constants = new Dictionary<string, string>();

            var sansConstantDeclarations = new List<string>();
            foreach (var line in lines)
            {
                if (line.StartsWith("@constants:", StringComparison.InvariantCultureIgnoreCase))
                {
                    var declaredConstantsAndValues = line.Substring(11).Split(',');
                    foreach (var cav in declaredConstantsAndValues)
                    {
                        var constantAndValue = cav.Split('=');
                        var constant = constantAndValue.First().Trim();
                        var value = constantAndValue.Last().Trim();
                        constants.Add(constant, value);
                    }
                }
                else
                    sansConstantDeclarations.Add(line);
            }

            var withConstantInlining = new List<string>();
            foreach (var line in sansConstantDeclarations)
            {
                var newLine = line;
                foreach (var av in constants)
                    newLine = newLine.Replace(av.Key, av.Value);
                withConstantInlining.Add(newLine);
            }
            return withConstantInlining;
        }

        List<string> SubstituteLabelsLineNumbers(List<string> lines)
        {
            //identify lines with label declarations and add line numbers:
            var labelToLineNumber = new Dictionary<string, string>();
            var lineNumber = 0;
            var withLineNumbers = new List<string>();
            foreach (var line in lines)
            {
                var newLine = line;
                if (newLine.StartsWith(":"))
                {
                    var labelEnd = newLine.TakeWhile(c => !char.IsWhiteSpace(c)).Count();
                    string label;
                    if (labelEnd < newLine.Length)
                    {
                        label = newLine.Substring(1, labelEnd).Trim();
                        newLine = newLine.Substring(labelEnd).Trim();
                    }
                    else if (labelEnd > 1)
                    {
                        label = newLine.Substring(1);
                        newLine = "rem";
                    }
                    else
                        throw new ArgumentException("Let's cross this particular bridge when we get to it");
                    labelToLineNumber.Add(label, lineNumber.ToString());
                }

                withLineNumbers.Add($"{lineNumber} {newLine}");
                lineNumber++;
            }

            //replace all occurences of given labels with their corresponding line number:
            var sansLabels = new List<string>();
            foreach (var line in withLineNumbers)
            {
                var newLine = line;
                foreach (var ltl in labelToLineNumber)
                    newLine = newLine.Replace(ltl.Key, ltl.Value);
                sansLabels.Add(newLine);
            }

            return sansLabels;
        }
    }
}
