namespace ControlStrings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ControlStringFinder : IControlStringFinder
    {
        readonly char controlStringStarter;
        readonly char controlStringTerminator;
        readonly char specialStringStarter;
        readonly char specialStringTerminator;
        readonly char transformerSeparator;
        readonly char valueSeparator;

        public ControlStringFinder
        (
            char controlStringStarter,
            char valueSeparator,
            char controlStringTerminator,
            char specialStringStarter,
            char specialStringTerminator,
            char transformerSeparator
        )
        {
            this.controlStringStarter = controlStringStarter;
            this.valueSeparator = valueSeparator;
            this.controlStringTerminator = controlStringTerminator;
            this.specialStringStarter = specialStringStarter;
            this.specialStringTerminator = specialStringTerminator;
            this.transformerSeparator = transformerSeparator;
        }

        public IEnumerable<ControlString> FindAllControlStrings(string input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // Yield return methods doesn't throw any exceptions until you try to access the results
            // This could happen at any time, including far away from when the error was caused
            // So we pass error handle normally here and contain the yield return in its own bubble
            return FindAllControlStringsEnumeratorInternal(input);
        }

        IEnumerable<ControlString> FindAllControlStringsEnumeratorInternal(string input)
        {
            for (var index = 0; index < input.Length; index++)
            {
                if (input[index] == controlStringStarter)
                {
                    var end = FindControlStringEnd(input, index);

                    var internalString = input.Substring(index + 1, end - index - 1);

                    var prependSpecial = FindPrependingSpecial(internalString);
                    var postpendSpecial = FindPostpendingSpecial(internalString);

                    int prependLength = prependSpecial.Length == 0 ? 0 : prependSpecial.Length + 2;
                    int postpendLength = postpendSpecial.Length == 0 ? 0 : postpendSpecial.Length + 2;

                    var separatedControlStrings = internalString.Substring(prependLength, internalString.Length - postpendLength - prependLength).Split(valueSeparator);
                    var finalControlStringSplitByTransformerSeparator = separatedControlStrings.Last().Split(transformerSeparator);
                    var values = new Queue<string>(separatedControlStrings.Take(separatedControlStrings.Length-1).Concat(new[] { finalControlStringSplitByTransformerSeparator.First() }));
                    var transformers = new Queue<string>(finalControlStringSplitByTransformerSeparator.Skip(1));

                    yield return new ControlString(index, end - index + 1, values, transformers);

                    index = end;
                }
            }
        }

        private int FindControlStringEnd(string input, int startingIndex)
        {
            for (var index = startingIndex + 1; index < input.Length; index++)
            {
                if (input[index] == controlStringTerminator)
                {
                    return index;
                }
            }

            throw new FormatException("Input string has opening control string starter with no matching control string terminator.");
        }

        public string FindPostpendingSpecial(string input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input == string.Empty)
            {
                return string.Empty;
            }

            if (input[input.Length - 1] != specialStringTerminator)
            {
                return string.Empty;
            }

            for (var index = input.Length - 2; index >= 0; index--)
            {
                if (input[index] == specialStringStarter)
                {
                    return input.Substring(index + 1, input.Length - index - 2);
                }
            }

            return string.Empty;
        }

        public string FindPrependingSpecial(string input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input == string.Empty)
            {
                return string.Empty;
            }

            if (input[0] != specialStringStarter)
            {
                return string.Empty;
            }

            for (var index = 1; index < input.Length; index++)
            {
                if (input[index] == specialStringTerminator)
                {
                    return input.Substring(1, index - 1);
                }
            }

            return string.Empty;
        }
    }
}