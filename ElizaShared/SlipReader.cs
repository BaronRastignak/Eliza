using System.Text;
using Eliza.Cells;

namespace Eliza;

public static class SlipReader
{
    private const string DescriptionListPrefix = "DLIST";

    /// <summary>
    /// Reads string into a list (parentheses inside the list are considered literals)
    /// </summary>
    /// <param name="buffer">List to which string is read</param>
    /// <param name="inputString">Input string representing a list</param>
    public static void TextRead(SlipList? buffer, string? inputString)
    {
        if (buffer == null || string.IsNullOrEmpty(inputString))
            return;

        var sb = new StringBuilder();
        foreach (var ch in inputString)
            if (!char.IsWhiteSpace(ch))
            {
                sb.Append(ch);
            }
            else if (sb.Length > 0)
            {
                buffer.NewBottom(sb.ToString());
                sb.Clear();
            }
    }

    /// <summary>
    /// Read string representation of the list into a list proper. List structure and description list
    /// (if any) is preserved. List representation must be enclosed in parentheses.
    /// </summary>
    /// <param name="buffer">List to which string is read</param>
    /// <param name="inputStream">Input stream representing a list</param>
    /// <param name="skipOpening">Skip opening parenthesis on recursive read</param>
    /// <returns>Number of characters read from <paramref name="inputStream"/></returns>
    /// <exception cref="FormatException">Input data is not enclosed in a matching set of parentheses</exception>
    public static void ListRead(SlipList? buffer, TextReader inputStream, bool skipOpening = false)
    {
        if (buffer is null)
            return;

        int ch;

        if (!skipOpening)
        {
            do
            {
                ch = inputStream.Read();
            } while (ch >= 0 && char.IsWhiteSpace((char) ch));

            if (ch != '(')
                throw new FormatException("Input is not recognized as valid list - '(' expected");
        }

        var sb = new StringBuilder();
        while ((ch = inputStream.Read()) >= 0)
            if (char.IsWhiteSpace((char) ch))
            {
                if (sb.Length <= 0)
                    continue; // skip consecutive whitespaces

                var datum = sb.ToString();
                sb.Clear();
                if (datum.Equals(DescriptionListPrefix, StringComparison.OrdinalIgnoreCase))
                    ReadDescriptionList(buffer.MakeDescriptionList(), inputStream);
                else
                    buffer.NewBottom(datum);
            }
            else
            {
                switch (ch)
                {
                    case '(':
                        ListRead(buffer.NewBottom(new SlipList()), inputStream, true);
                        break;

                    case ')':
                        if (sb.Length > 0)
                            buffer.NewBottom(sb.ToString());

                        return;

                    default:
                        sb.Append((char) ch);
                        break;
                }
            }

        throw new FormatException("Unexpected end of input - ')' expected");
    }

    /// <summary>
    /// Wrapper for <see cref="ListRead"/> to take care of possible whitespaces between
    /// <see cref="DescriptionListPrefix"/> and the description list text representation
    /// </summary>
    /// <param name="descriptionList">Description list to fill</param>
    /// <param name="inputString">String representation of the description list</param>
    /// <returns>Number of characters read from the string including whitespaces in the beginning</returns>
    private static void ReadDescriptionList(SlipList descriptionList, TextReader inputString)
    {
        int ch;
        do
        {
            ch = inputString.Read();
            if (ch < 0)
                throw new FormatException("Unexpected end of input - no description list found");
        } while (char.IsWhiteSpace((char) ch));

        if (ch != 0)
            throw new FormatException("No valid description list found - '(' expected.");

        ListRead(descriptionList, inputString, true);
    }

    /// <summary>
    /// Prints list as a linear text string (single-level list is expected)
    /// </summary>
    /// <param name="list">List to be printed</param>
    /// <returns>String representation of the list (without enclosing parentheses)</returns>
    public static string TextPrint(SlipList? list)
    {
        if (list == null)
            return string.Empty;

        var result = new List<string>();
        var reader = list.GetSequentialReader();

        while (reader.SequenceLinearElementRight() is { } datumCell)
            result.Add(datumCell.Datum);

        return string.Join(" ", result);
    }

    public static string ListPrint(SlipList? list)
    {
        if (list == null)
            return string.Empty;

        var sb = new StringBuilder("(");
        if (list.DescriptionList != null)
            sb.Append($"{DescriptionListPrefix} ({TextPrint(list.DescriptionList)})");

        var reader = list.GetSequentialReader();
        while (reader.SequenceLinearRight() is not SlipCellHeader)
            if (reader.Cell is SlipCellDatum datumCell)
            {
                if (sb.Length > 1)
                    sb.Append(" ");

                sb.Append(datumCell.Datum);
            }
            else if (reader.Cell is SlipCellListName nameCell)
            {
                if (sb.Length > 1)
                    sb.Append(" ");

                sb.Append(ListPrint(nameCell.List));
            }

        sb.Append(")");
        return sb.ToString();
    }
}