using System.Globalization;
using System.Text;
using Eliza.Cells;

namespace Eliza;

/// <summary>
/// Methods to process keywords and transform user input into ELIZA output
/// </summary>
public static class SlipProcessor
{
    public static void Change(SlipList[] keyList, SlipList noneList, SlipList[] memoryTransforms)
    {
        var input = new SlipList();
        var commands = new[] { "TYPE", "SUBSTITUTE", "APPEND", "ADD", "START", "RANK", "DISPLAY" };
        var descriptions = new[]
        {
            "<keyword> - print out rules associated with this keyword.",
            "<keyword> <old rule list> <new rule list> - replace the old rule associated with the keyword with the new rule",
            "<keyword> <old rule list> <new rule list> - append the new rule associated with the keyword after the old rule",
            "<keyword> <new rule list> - add the new rule associated with the keyword to the end of the rules list",
            "- exit the edit mode and return to running script.",
            "<keyword> <new rank> - change rank (precedence) for the given keyword",
            "- display current script."
        };
        Console.WriteLine("Recognized commands:");
        for (var i = 0; i < commands.Length; i++)
            Console.WriteLine($"\t{commands[i]} {descriptions[i]}");

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Please instruct me:");
            SlipReader.ListRead(input.EmptyList(), new StringReader(Console.ReadLine() ?? "()"));

            var job = (input.PopTop() as SlipCellDatum)?.Datum;
            var jobNumber = 0;
            for (var i = 0; i < commands.Length; i++)
                if (commands[i].Equals(job, StringComparison.CurrentCultureIgnoreCase))
                {
                    jobNumber = i + 1;
                    break;
                }

            if (jobNumber == 0)
            {
                Console.WriteLine("Change not recognized");
                continue;
            }

            switch (jobNumber)
            {
                case 5:
                    return;

                case 7:
                    for (var i = 0; i < keyList.Length; i++)
                    {
                        if (keyList[i].IsEmpty)
                            continue;

                        var sReader = keyList[i].GetSequentialReader();
                        while (sReader.SequenceLinearNameRight() is { } nextName)
                        {
                            var next = nextName.List;
                            Console.WriteLine("*");
                            Console.Write(TextPrint(next));
                            Console.WriteLine($" {i:###} ");
                        }
                    }

                    if (!noneList.IsEmpty)
                    {
                        var sReader = noneList.GetSequentialReader();
                        while (sReader.SequenceLinearNameRight() is { } nextName)
                        {
                            Console.WriteLine("*");
                            Console.Write(TextPrint(nextName.List));
                            Console.WriteLine($" {keyList.Length:###} ");
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine("Memory list follows:");
                    Console.WriteLine();
                    foreach (var transform in memoryTransforms)
                        Console.WriteLine(SlipReader.TextPrint(transform));

                    break;
            }

            var theme = (input.PopTop() as SlipCellDatum)?.Datum;
            if (string.IsNullOrEmpty(theme))
                continue;

            var subject = keyList[Hash(theme, 5)];
            var seqReader = subject.GetSequentialReader();
            SlipList? term = null;
            while (seqReader.SequenceLinearNameRight() is { } termName)
            {
                term = termName.List;
                if (theme.Equals((term.Top as SlipCellDatum)?.Datum, StringComparison.CurrentCultureIgnoreCase))
                    break;
            }

            if (term is null || seqReader.Cell is SlipCellHeader)
            {
                Console.WriteLine("List not found");
                continue;
            }

            switch (jobNumber)
            {
                case 1:
                    TextPrint(term);
                    break;

                case 2:
                case 3:
                {
                    seqReader = term.GetSequentialReader();
                    var old = (input.PopTop() as SlipCellListName)?.List;
                    var ok = false;
                    if (old is not null)
                        while (seqReader.SequenceLinearNameRight() is { } objName)
                        {
                            var obj = objName.List;
                            var inside = obj.GetSequentialReader();
                            while (inside.SequenceLinearNameRight() is { } itName)
                            {
                                var it = itName.List;
                                var seqIt = it.GetSequentialReader();
                                var sOld = old.GetSequentialReader();

                                ok = true;
                                while (sOld.SequenceLinearElementRight() is { } datOld &&
                                       seqIt.SequenceLinearElementRight() is { } datIt)
                                    if (!datOld.Datum.Equals(datIt.Datum, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        ok = false;
                                        break;
                                    }

                                if (ok && input.PopTop() is SlipCellListName inpName)
                                {
                                    if (jobNumber == 2)
                                        inside.Cell.Substitute(inpName.List);
                                    else
                                        obj.NewBottom(inpName.List);

                                    break;
                                }
                            }

                            if (ok)
                                break;
                        }

                    if (!ok)
                        Console.WriteLine("List not found");
                }

                    break;

                case 4:
                    if (term.Bottom is SlipCellListName)
                    {
                        var bottom = term.PopBottom() as SlipCellListName;
                        if (input.PopTop() is SlipCellListName inpTop)
                            term.NewBottom(inpTop.List);

                        if (bottom is not null)
                            term.NewBottom(bottom.List);
                    }
                    else if (input.PopTop() is SlipCellListName inpTop)
                    {
                        term.NewBottom(inpTop.List);
                    }

                    break;

                case 6:
                {
                    seqReader = term.GetSequentialReader();
                    SlipCellBase? obj = seqReader.SequenceLinearNameRight();
                    if (obj is null)
                    {
                        Console.WriteLine("List not found");
                        continue;
                    }

                    obj = seqReader.SequenceLinearLeft();
                    if (input.PopTop() is SlipCellDatum inpDatum)
                    {
                        if (obj.Left is SlipCellHeader)
                            obj.Substitute(inpDatum.Datum);
                        else
                            obj.NewTop(inpDatum.Datum);
                    }
                }

                    break;
            }
        }
    }

    private static string TextPrint(SlipList list)
    {
        var sa = list.GetSequentialReader();
        var result = new StringBuilder();
        var output = new SlipList();
        while (sa.SequenceLinearRight() is not SlipCellHeader)
        {
            switch (sa.Cell)
            {
                case SlipCellDatum datumCell:
                    output.NewBottom(datumCell.Datum);
                    break;

                case SlipCellListName listName:
                    var next = listName.List;
                    result.Append(SlipReader.TextPrint(output));
                    sa.SequenceLinearLeft();

                    while (sa.SequenceLinearRight() is not SlipCellHeader)
                    {
                        if ((next.Top as SlipCellDatum)?.Datum == "=")
                        {
                            result.Append(SlipReader.TextPrint(next));
                            continue;
                        }

                        result.AppendLine();
                        var sb = next.GetSequentialReader();
                        while (sb.SequenceLinearRight() is not SlipCellHeader)
                        {
                            switch (sb.Cell)
                            {
                                case SlipCellDatum scd:
                                {
                                    if (int.TryParse(scd.Datum, out var term))
                                        result.Append($"{term:###} ");

                                    break;
                                }

                                case SlipCellListName ln:
                                {
                                    var term = ln.List;
                                    result.Append(SlipReader.TextPrint(term));
                                    break;
                                }
                            }
                        }
                    }

                    return result.ToString();
            }
        }

        result.Append(SlipReader.TextPrint(output));
        return result.ToString();
    }

    public static SequentialReader? Tests(SlipList candidate, SequentialReader sReader)
    {
        var store = sReader.Cell;
        var reader = candidate.GetSequentialReader();
        var first = reader.SequenceLinearElementRight()?.Datum;
        sReader.SequenceLinearLeft();
        var second = sReader.SequenceLinearElementRight()?.Datum;

        if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second) ||
            !string.Equals(first, second, StringComparison.CurrentCultureIgnoreCase))
            return null;

        var equal = (reader.SequenceLinearRight() as SlipCellDatum)?.Datum;
        if (!string.Equals(equal, "=", StringComparison.Ordinal))
        {
            reader.SequenceLinearLeft();
            return reader;
        }

        var point = store.Left;
        store.Remove();
        var newDatum = reader.SequenceLinearElementRight()?.Datum;
        if (!string.IsNullOrEmpty(newDatum))
            point.NewBottom(newDatum);

        reader.SequenceLinearRight();
        return reader;
    }

    public static void Rule(SlipList specifications, SlipList input, SlipList result)
    {
        var specReader = specifications.GetSequentialReader();
        SlipCellDatum? datumCell;
        do
        {
            datumCell = specReader.SequenceLinearElementRight();
            if (datumCell is null)
                return;
        } while (!IsMatch("=", datumCell));

        var assemblySpecs = specifications.NewListRight(datumCell);
        assemblySpecs.PopTop();

        var interim = new SlipList();
        if (!YieldMatch(specifications, input, interim))
            return;

        Assemble(assemblySpecs, interim, result);
    }

    /// <summary>
    /// Assemble the <paramref name="result"/> out of elements of the <paramref name="input"/>
    /// according to the <paramref name="specification"/>
    /// </summary>
    /// <param name="specification">
    /// List of numeric specifications meaning to append the n-th element of the <paramref name="input"/>
    /// to the <paramref name="result"/>, and string literals to be appended to the <paramref name="result"/> as is.
    /// </param>
    /// <param name="input">Input data</param>
    /// <param name="result">Assembled list</param>
    public static void Assemble(SlipList specification, SlipList input, SlipList result)
    {
        var specReader = specification.GetSequentialReader();
        while (specReader.SequenceLinearElementRight() is { } specCell)
            if (int.TryParse(specCell.Datum, out var count) && count > 0)
            {
                // Numeric spec means to find the corresponding element in the input and append it
                // to the result flattening the sublist if needed
                var inputReader = input.GetSequentialReader();
                for (var i = 0; i < count; i++)
                {
                    inputReader.SequenceLinearRight();

                    if (inputReader.Cell is SlipCellHeader)
                        inputReader.SequenceLinearRight();
                }

                switch (inputReader.Cell)
                {
                    case SlipCellDatum datumCell:
                        result.NewBottom(datumCell.Datum);
                        break;

                    case SlipCellListName listName:
                        result.InsertRight(listName.List);
                        break;
                }
            }
            else
            {
                // Literal spec is appended to the result as is
                result.NewBottom(specCell.Datum);
            }
    }

    /// <summary>
    /// Divides <paramref name="input"/> into segments according to the specification
    /// in <paramref name="rule"/> and appends these segments to the <paramref name="result"/>.
    /// </summary>
    /// <param name="rule">List of specifications</param>
    /// <param name="input">Input data</param>
    /// <param name="result">List to save the results of input processing to.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="input"/> was successfully processed according to <paramref name="rule"/>,
    /// <see langword="false"/> otherwise
    /// </returns>
    public static bool YieldMatch(SlipList rule, SlipList input, SlipList result)
    {
        var ruleReader = rule.GetSequentialReader();
        var inputReader = input.GetSequentialReader();
        SlipCellDatum? spec;

        while ((spec = ruleReader.SequenceLinearElementRight()) is not null)
        {
            if (int.TryParse(spec.Datum, out var requiredNumber))
            {
                switch (requiredNumber)
                {
                    case < 0:
                        return false; // negative number is an error in spec

                    case 0:
                    {
                        var ruleCopy = ruleReader.ReaderCopy();
                        var nextRule = ruleCopy.SequenceLinearElementRight();

                        while (nextRule is not null && int.TryParse(nextRule.Datum, out var nextSegmentLength))
                        {
                            if (nextSegmentLength == 0)
                                return false; // no way to correctly decompose pattern of numbers between zeroes

                            requiredNumber += nextSegmentLength;
                            nextRule = ruleCopy.SequenceLinearElementRight();
                        }

                        if (nextRule is not null)
                        {
                            // a word is found, let's check if the pattern is buildable
                            var testReader = inputReader.ReaderCopy();
                            var nextInput = testReader.SequenceLinearElementRight();
                            while (!IsMatch(nextRule.Datum, nextInput))
                            {
                                if (nextInput is null)
                                    return false; // whoops, no such word in the input

                                requiredNumber--;
                                nextInput = testReader.SequenceLinearElementRight();
                            }

                            switch (requiredNumber)
                            {
                                case > 0:
                                    return false; // requested word was found too soon

                                case 0:
                                    result.NewBottom(new SlipList());
                                    continue; // word is found, but '0' spec is empty this time

                                default:
                                    requiredNumber = -requiredNumber;
                                    break;
                            }
                        }
                        else if (requiredNumber != 0)
                        {
                            // pattern is zero with some number afterwards. Find the length of the zero segment
                            var tailLength = CalcTailLength(inputReader);
                            requiredNumber = tailLength - requiredNumber;
                        }

                        if (!AppendSegment(requiredNumber, inputReader, result))
                            return false;

                        break;
                    }

                    default:
                        if (!AppendSegment(requiredNumber, inputReader, result))
                            return false; // not enough elements to append

                        break;
                }
            }
            else
            {
                // string literal spec
                var testReader = inputReader.ReaderCopy();
                var datumCell = testReader.SequenceLinearElementRight();
                if (!IsMatch(spec.Datum, datumCell))
                    return false;

                // append this literal to the result list
                AppendSegment(1, inputReader, result);
            }
        }

        return true;
    }

    private static bool IsMatch(string spec, SlipCellDatum? datumCell)
    {
        return string.Equals(spec, datumCell?.Datum, StringComparison.OrdinalIgnoreCase);
    }

    private static bool AppendSegment(int segmentLength, SequentialReader inputReader, SlipList segments)
    {
        if (segmentLength < 0)
            return false;

        var sublist = new SlipList();
        if (segmentLength == 0)
        {
            var cell = inputReader.SequenceLinearElementRight();
            while (cell is not null)
            {
                sublist.NewBottom(cell.Datum);
                cell = inputReader.SequenceLinearElementRight();
            }
        }
        else
        {
            for (int i = 0; i < segmentLength; i++)
            {
                var cell = inputReader.SequenceLinearElementRight();
                if (cell is null)
                    return false;

                sublist.NewBottom(cell.Datum);
            }
        }

        segments.NewBottom(sublist);
        return true;
    }

    private static int CalcTailLength(SequentialReader reader)
    {
        var length = 0;
        var readerCopy = reader.ReaderCopy();
        while (readerCopy.SequenceLinearElementRight() is not null)
            length++;

        return length;
    }

    public static int Hash(string datum, int rank)
    {
        if (datum.Length % 6 != 0)
            datum = datum.PadRight((datum.Length / 6 + 1) * 6);

        var hashSource = datum[^6..];
        var encoded = hashSource.Select(EncodeHollerith);
        return Hash(Convert.ToUInt64(string.Join("", encoded), 8), rank);
    }

    /// <summary>
    /// Recreate the SLIP HASH function: return an n-bit hash value for
    /// the given 36-bit datum d, for values of n in range 0..15
    /// </summary>
    /// <remarks>From https://github.com/anthay/ELIZA/blob/master/doc/Eliza_part_3.md</remarks>
    private static int Hash(ulong d, int n)
    {
        d &= 0x7FFFFFFFFul; // clear the "sign" bit
        d *= d; // square it
        d >>= 35 - n / 2; // move middle n bits to least sig. bits
        return (int) (d & ((1ul << n) - 1)); // mask off all but n least sig. bits
    }

    private static int? EncodeHollerith(char c)
    {
        switch (c)
        {
            case '0': return 0;
            case '1': return 1;
            case '2': return 2;
            case '3': return 3;
            case '4': return 4;
            case '5': return 5;
            case '6': return 6;
            case '7': return 7;
            case '8': return 10;
            case '9': return 11;
            case '=': return 13;
            case '\'':
            case '"':
                return 14;
            case '+': return 20;
            case 'A':
            case 'a':
                return 21;
            case 'B':
            case 'b':
                return 22;
            case 'C':
            case 'c':
                return 23;
            case 'D':
            case 'd':
                return 24;
            case 'E':
            case 'e':
                return 25;
            case 'F':
            case 'f':
                return 26;
            case 'G':
            case 'g':
                return 27;
            case 'H':
            case 'h':
                return 30;
            case 'I':
            case 'i':
                return 31;
            case '.': return 33;
            case ')': return 34;
            case '-': return 40;
            case 'J':
            case 'j':
                return 41;
            case 'K':
            case 'k':
                return 42;
            case 'L':
            case 'l':
                return 43;
            case 'M':
            case 'm':
                return 44;
            case 'N':
            case 'n':
                return 45;
            case 'O':
            case 'o':
                return 46;
            case 'P':
            case 'p':
                return 47;
            case 'Q':
            case 'q':
                return 50;
            case 'R':
            case 'r':
                return 51;
            case '$': return 53;
            case '*': return 54;
            case ' ': return 60;
            case '/': return 61;
            case 'S':
            case 's':
                return 62;
            case 'T':
            case 't':
                return 63;
            case 'U':
            case 'u':
                return 64;
            case 'V':
            case 'v':
                return 65;
            case 'W':
            case 'w':
                return 66;
            case 'X':
            case 'x':
                return 67;
            case 'Y':
            case 'y':
                return 70;
            case 'Z':
            case 'z':
                return 71;
            case '#': return 72;
            case ',': return 73;
            case '(': return 74;

            default: return null;
        }
    }
}