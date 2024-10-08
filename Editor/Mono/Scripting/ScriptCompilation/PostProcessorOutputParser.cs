// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Text.RegularExpressions;
using UnityEditor.Scripting.Compilers;

namespace UnityEditor.Scripting.ScriptCompilation
{
    internal class PostProcessorOutputParser : CompilerOutputParserBase
    {
        private static Regex sCompilerOutput = new Regex(@"(?<filename>[^:]*)\((?<line>\d+),(?<column>\d+)\):\s*(?<type>warning|error)\s*(?<message>.*)", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        protected override bool ShouldParseLine(string line)
        {
            return line.Contains("warning", StringComparison.Ordinal) ||
                   line.Contains("error", StringComparison.Ordinal);
        }

        protected override string GetInformationIdentifier()
        {
            return default;
        }

        protected override Regex GetOutputRegex()
        {
            return sCompilerOutput;
        }

        protected override string GetErrorIdentifier()
        {
            return "error";
        }
    }
}
