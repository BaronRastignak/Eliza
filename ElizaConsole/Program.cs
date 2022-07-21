using Eliza;
using Eliza.Cells;

Console.WriteLine(@"
                      ELIZA
based on Joseph Weizenbaum's original version, 1966

               .NET translation by
             Andre de Rastignak, 2022
");

var keyList = new SlipList[32]; // keyword responses list
var noneList = new SlipList(); // list of answers to use when keyword isn't found
var memoryTransforms = new SlipList[4]; // memory transformation rules

string? fileName;
bool fileExists;
do
{
    Console.WriteLine("Which script do you want to play?");

    fileName = Console.ReadLine();
    fileExists = File.Exists(fileName);
    if (!fileExists)
        Console.WriteLine("Sorry, the file wasn't found, try again");
} while (fileName is null || !fileExists);

var test = new SlipList(); // stores the part of input tested against a decomposition rule
var input = new SlipList(); // buffer for ELIZA script text & user input
var output = new SlipList(); // ELIZA response buffer
var junk = new SlipList(); // temporary buffer

var limit = 1; // memory counter

// initialize memory lists
for (var i = 0; i < memoryTransforms.Length; i++)
    memoryTransforms[i] = new SlipList();

SlipList? mine = null;
var memoryList = new SlipList(); // memory buffer
var memory = string.Empty; // memory keyword

for (var i = 0; i < keyList.Length; i++)
    keyList[i] = new SlipList();

// SCRIPT ERROR phrases
var noMatch = new[]
{
    "Please continue",
    "Hmmm",
    "Go on, please",
    "I see"
};

// READ NEW SCRIPT
const string memoryPrefix = "MEMORY";
const string nonePrefix = "NONE";

using (var reader = File.OpenText(fileName))
{
    SlipReader.ListRead(junk, reader); // read the hello message (first list in the script)

    while (!reader.EndOfStream)
    {
        input.EmptyList();
        input.DropDescriptionList();
        SlipReader.ListRead(input, reader);

        if (input.IsEmpty)
        {
            Console.WriteLine(Environment.NewLine + "To modify the current script enter \"+\"");
            Console.WriteLine(
                "To add new transformation rule on the fly begin your sentence with \"* \" and write the new rule afterwards." +
                Environment.NewLine);

            Console.WriteLine(new string('-', 25) + Environment.NewLine);

            Console.WriteLine(SlipReader.TextPrint(junk));
            junk.EmptyList();
            break;
        }

        var top = (input.Top as SlipCellDatum)?.Datum;
        if (string.Equals(top, nonePrefix, StringComparison.OrdinalIgnoreCase))
        {
            noneList.NewTop(input.ListCopy());
        }
        else if (string.Equals(top, memoryPrefix, StringComparison.OrdinalIgnoreCase))
        {
            input.PopTop();
            if (input.PopTop() is SlipCellDatum datumCell)
                memory = datumCell.Datum;

            for (var i = 0; i < memoryTransforms.Length; i++)
                if (input.PopTop() is SlipCellListName listNameCell)
                    memoryTransforms[i] = listNameCell.List.ListCopy();
        }
        else if (!string.IsNullOrEmpty(top))
        {
            // add a keyword to the dictionary
            keyList[SlipProcessor.Hash(top, 5)].NewBottom(input.ListCopy());
        }
    }
}

// BEGIN MAJOR LOOP
while (true)
{
    SlipReader.TextRead(input.EmptyList(), Console.ReadLine());

    var keyword = string.Empty; // keyword found with the highest precedence
    var precedence = 0; // maximum keyword precedence found

    limit++;
    if (limit == 5)
        limit = 1;

    if (input.IsEmpty)
    {
        // DUMP REVISED SCRIPT
        Console.Write($"What is to be name of the new script? [{fileName}] ");
        var scriptName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(scriptName))
            scriptName = fileName;

        using var script = File.CreateText(scriptName);

        SlipReader.TextRead(input, File.ReadLines(fileName).First());
        script.WriteLine(SlipReader.TextPrint(input));

        output.EmptyList().NewTop(memory);
        output.NewTop(memoryPrefix);

        foreach (var transform in memoryTransforms)
            output.NewBottom(transform);

        script.WriteLine(SlipReader.ListPrint(output));
        output.EmptyList();

        foreach (var keyRules in keyList)
            while (!keyRules.IsEmpty)
                if (keyRules.PopTop() is SlipCellListName listName)
                    script.WriteLine(SlipReader.ListPrint(listName.List));

        script.WriteLine(SlipReader.ListPrint(input.EmptyList()));

        break;
    }

    SequentialReader? it = null; // sequence reader for selected transformation rule or null if no keyword is found
    var top = input.Top as SlipCellDatum;
    switch (top?.Datum)
    {
        case "+":
            SlipProcessor.Change(keyList, noneList, memoryTransforms);
            continue;
        case "*":
        {
            input.PopTop();
            if (input.Top is SlipCellDatum datumCell)
                keyList[SlipProcessor.Hash(datumCell.Datum, 5)].NewBottom(input.ListCopy());

            continue;
        }
    }

    var sReader = input.GetSequentialReader();
    while (sReader.SequenceLinearElementRight() is { } cell)
    {
        var word = cell.Datum;
        // next is probably a bug in the original code: "." or "," can be a distinct word only if user enters
        // whitespace before them
        if (word is "." or "," || string.Equals(word, "BUT", StringComparison.OrdinalIgnoreCase))
        {
            if (it is null)
            {
                junk = input.NewListLeft(sReader.Cell);
                junk.EmptyList();
                continue;
            }

            junk = input.NewListRight(sReader.Cell);
            junk.EmptyList();
            break;
        }

        var i = SlipProcessor.Hash(word, 5);
        var scanner = keyList[i].GetSequentialReader();
        var found = false;
        SlipList? candidate = null;
        while (scanner.SequenceLinearNameRight() is { } candCell)
        {
            candidate = candCell.List;
            if (string.Equals((candidate.Top as SlipCellDatum)?.Datum, word,
                    StringComparison.CurrentCultureIgnoreCase))
            {
                found = true;
                break;
            }
        }

        if (candidate is null || !found)
            continue;

        var reader = SlipProcessor.Tests(candidate, sReader);
        if (reader is null)
            continue;

        var dList = candidate.DescriptionList;
        if (dList is not null)
            sReader.SequenceLinearRight().NewTop(dList);

        var next = reader.SequenceLinearRight();
        if (it is null && next is SlipCellListName)
        {
            it = reader;
            keyword = word;
        }
        else if (next is SlipCellDatum datumCell && int.TryParse(datumCell.Datum, out var nextPrecedence) &&
                 nextPrecedence > precedence)
        {
            precedence = nextPrecedence;
            it = reader;
            keyword = word;
        }
    }

    SlipList? elizaSequense = null;
    if (it is null)
    {
        if (limit == 4 && !memoryList.IsEmpty)
        {
            var outputMemory = memoryList.PopTop() as SlipCellListName;
            Console.WriteLine(SlipReader.TextPrint(outputMemory?.List));
            continue;
        }

        elizaSequense = ((noneList.Top as SlipCellListName)?.List.Bottom as SlipCellListName)?.List;
    }
    else if (keyword.Equals(memory, StringComparison.CurrentCultureIgnoreCase) &&
             input.Bottom is SlipCellDatum inputBottom)
    {
        var i = SlipProcessor.Hash(inputBottom.Datum, 2);
        mine = new SlipList();
        SlipProcessor.Rule(memoryTransforms[i], input, mine);
        memoryList.NewBottom(mine);
        it.SequenceLinearLeft();
    }
    else
    {
        it.SequenceLinearLeft();
    }

    // MATCHING ROUTINE
    while (elizaSequense is null || (elizaSequense.Top is SlipCellListName esTop &&
                                     !SlipProcessor.YieldMatch(esTop.List, input, test.EmptyList())))
    {
        var esName = it?.SequenceLinearNameRight();
        if (esName is null)
        {
            elizaSequense = null;
            break;
        }

        elizaSequense = esName.List;
        if (elizaSequense.Top is SlipCellDatum { Datum: "=" })
        {
            sReader = elizaSequense.GetSequentialReader();
            sReader.SequenceLinearRight();
            var word = sReader.SequenceLinearElementRight()?.Datum;
            if (!string.IsNullOrEmpty(word))
            {
                var i = SlipProcessor.Hash(word, 5);
                var scanner = keyList[i].GetSequentialReader();

                var matchFound = false;
                while (scanner.SequenceLinearNameRight() is { } itsName)
                {
                    var itSelected = itsName.List;
                    if (itSelected.Top is SlipCellDatum itsDatum &&
                        itsDatum.Datum.Equals(word, StringComparison.CurrentCultureIgnoreCase))
                    {
                        sReader = itSelected.GetSequentialReader();
                        elizaSequense = sReader.SequenceLinearNameRight()?.List;
                        it = sReader;
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    elizaSequense = null;
                    break;
                }
            }
        }
    }


    if (elizaSequense is null)
    {
        Console.WriteLine(noMatch[limit - 1]);
        continue;
    }

    var elizaSequentialReader = elizaSequense.GetSequentialReader();
    var pointer = elizaSequentialReader.SequenceLinearRight();
    var point = elizaSequentialReader.SequenceLinearRight();
    SlipList? transformation = null;
    switch (point)
    {
        case SlipCellListName pName:
            pointer.NewBottom("1");
            transformation = pName.List;
            break;

        case SlipCellDatum pDatum when int.TryParse(pDatum.Datum, out var count):
        {
            for (var i = 0; i <= count; i++)
                transformation = elizaSequentialReader.SequenceLinearNameLeft()?.List;

            if (transformation is null)
            {
                elizaSequentialReader.SequenceLinearRight();
                elizaSequentialReader.SequenceLinearRight();
                transformation = elizaSequentialReader.SequenceLinearNameRight()?.List;
                pDatum.Substitute("1");
            }
            else
            {
                pDatum.Substitute((count + 1).ToString());
            }

            break;
        }
    }

    if (transformation is not null)
    {
        SlipProcessor.Assemble(transformation, test, output.EmptyList());
        Console.WriteLine(SlipReader.TextPrint(output));
    }
}
