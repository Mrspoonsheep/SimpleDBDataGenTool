using Microsoft.Identity.Client;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace DBGenerator
{
    /* 
    -ALL- comments in this document are manually written, however, AI has been used to generate some chunks of code for speeds sake.
    In some of these cases the AI had inserted comments, these have all been removed and reworked manually to work better in context
    and make sure that the information is correct. Anything AI generated has comments marking that fact, as well as everything
    that started as AI to get an outline of which to continue from.

    Cheers :)
     */
    internal class Program
    {
        public static Csv[] csvList = new Csv[9];
        public static CsvValues csvValues { get; set; }
        public static string CSVSourcePath = "F:\\DBcsv"; // where to find source csv files.
        public static string CSVOutputPath = "F:\\DBcsv\\Output"; // where to write the new CSV files to.
        public enum Passwords { MySQL = 0, PostgreSQL = 1 }
        public enum id { file = 0, column = 1 } // enum created for readability when using the 'fileandcolumn' integer arrays for identification
        public static int insertCounter = 0;

        static void Main(string[] args)
        {
            bool valid = false;
            bool maxDataHoldersReached = false;
            //receive locations of CSV source files.
            while (!valid)
            {
                Console.Clear();
                Console.WriteLine("Please input path for CSV source files!");

                Console.Write("Directory Path: ");
                CSVSourcePath = Console.ReadLine();

                if (!Directory.Exists(CSVSourcePath))
                {
                    continue;
                }
                valid = true;
            csvValues = new CsvValues(CSVSourcePath);
            }
            valid = false;
            while (!valid)
            {
                Console.Clear();
                Console.WriteLine("Please Iutput output path for new CSV files!");

                Console.Write("Directory Path: ");
                CSVOutputPath = Console.ReadLine();

                if (!Directory.Exists(CSVOutputPath)) // check if path doesn't exists
                {
                    continue; // skip to next iteration
                }
                valid = true;
            }
            while (true) // main program loop
            {
                Console.Clear();
                Console.WriteLine("Welcome to the DB Data Generation Tool!");

                Console.WriteLine($"\nFile Source: {CSVSourcePath}");
                Console.WriteLine($"\nFile Out: {CSVOutputPath}");
                Console.Write("\nData Holders: ");

                if (insertCounter == 0)
                {
                    Console.Write("WARNING: No data holders have been created!");
                }

                foreach (Csv csv in csvList) // list all CSVs currently stored in memory (I tried to not have to store them in memory all at once before pushing to disk, but it wasn't worth the work)
                {
                    if (csv.Rows != null)
                        Console.Write($"{csv.fileName}  ");
                }
                if (maxDataHoldersReached) // notify user if CSV array is full
                {
                    Console.Write("WARNING: Max data holders reached!");
                }
                // displays the menu
                Console.WriteLine("\n\nOptions:");
                Console.Write("\n");
                Console.WriteLine("s) fraud Social Security Numbers (random 9 digit number)");
                Console.WriteLine("f) fraud licence plates (random number)");
                Console.WriteLine("a) addresses '[letter]Street####'");
                Console.WriteLine("0) dates (requires first 2 columns to be empty)");
                Console.WriteLine("1) Names");
                Console.WriteLine("2) manufacturers");
                Console.WriteLine("3) Licence Plates");
                Console.WriteLine("4) Social Security Numbers");

                Console.WriteLine("\n5) Create File-Data Holder");

                Console.WriteLine("\n6) Compose Composite");
                Console.WriteLine("7) Compose Composite with Manual Entry Numbers");

                Console.WriteLine("\n8) Push To Files");

                Console.WriteLine("\nq) Quit");

                Console.Write("\n\n>Press Key<");
                ConsoleKeyInfo key = Console.ReadKey(); // read keypresses
                char switchKey = key.KeyChar;

                switch (switchKey) // handle keypresses
                {
                    case 's':
                        GenerateFraudSSN(HandleSelection());
                        continue;
                    case 'f':
                        GenerateFraudLicencePlates(HandleSelection());
                        continue;
                    case 'a':
                        GenerateAddresses(HandleSelection());
                        continue;
                    case '0':
                        GenerateDates(HandleSelection());
                        continue;
                    case '1':
                        GenerateNames(HandleSelection());
                        continue;
                    case '2':
                        GenerateManufacturers(HandleSelection());
                        continue;
                    case '3':
                        GenerateLicencePlates(HandleSelection());
                        continue;
                    case '4':
                        GenerateSSN(HandleSelection());
                        continue;
                    case '5':
                        if (insertCounter < 9) // check if the CSV array is full
                        {
                            CreateDataHolder();
                            insertCounter++;
                        }
                        else
                        {
                            maxDataHoldersReached = true;
                        }
                        continue;
                    case '6':
                        constructComposite();
                        continue;
                    case '7':
                        if (insertCounter < 9) // check if the CSV array is full
                        {
                            constructCompositeManualEntries();
                            insertCounter++;
                        }
                        else
                        {
                            maxDataHoldersReached = true;
                        }
                        continue;
                    case '8':
                        PushToFiles();
                        continue;
                    case 'q':
                        Environment.Exit(0); // safely exit the program
                        continue;
                    default: continue;
                }


            }

        }

        static void GenerateFraudSSN(int[] fileAndColumn)
        {
            bool choiceConfirmed = false;
            bool triedNonNumeric = false;
            string stringAmmount = "";
            while (!choiceConfirmed)
            {
                //Get Ammount of data to generate
                Console.Clear();
                Console.WriteLine("CHOOSE AMMOUNT\n");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                Console.WriteLine($"\nfile name: {csvList[fileAndColumn[(int)id.file]].fileName}");


                Console.Write("Number of Social Security Numbers to generate:");
                stringAmmount = Console.ReadLine();

                if (HandleNumericInput(stringAmmount)) continue;

                while (!choiceConfirmed && !triedNonNumeric)
                {
                    Console.WriteLine($"Is {stringAmmount} correct?");

                    Console.WriteLine("\n[ENTER] Yes");
                    Console.WriteLine("[ESC] No (retype)\n\n");

                    Console.Write(">Press Key<");
                    choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
                }
            }
            int amount = int.Parse(stringAmmount);

            Generate generator = new();
            // check if file already has entries to detirmine correct method for insertion of data.
            if (csvList[fileAndColumn[(int)id.file]].Rows.Count <= 1)
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "SSN", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkAddColumn(generator.FraudSSN(amount), fileAndColumn[(int)id.column]);

            }
            // if file already has entries, use method for inserting into already existing spaces.
            else
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "SSN", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkInsertColumn(generator.FraudSSN(amount), fileAndColumn[(int)id.column]);
            }
        }
        static void GenerateAddresses(int[] fileAndColumn)
        {
            string stringAmount = "";
            bool choiceConfirmed = false;
            bool triedNonNumeric = false;

            while (!choiceConfirmed)
            {
                Console.Clear();
                Console.WriteLine("CHOOSE AMMOUNT\n\n");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                Console.Write($"\nnumber of entries: ");

                stringAmount = Console.ReadLine();

                if (HandleNumericInput(stringAmount)) continue;

                Console.Clear();

                Console.WriteLine($"Proceed with {stringAmount}?");

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
            }

            int amount = int.Parse(stringAmount);

            Generate generator = new Generate();
            // check if file already has entries to detirmine correct method for insertion of data.
            if (csvList[fileAndColumn[(int)id.file]].Rows.Count <= 1)
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "Address", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkAddColumn(generator.Addresses(amount), fileAndColumn[(int)id.column]);

            }
            // if file already has entries, use method for inserting into already existing spaces.
            else
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "Address", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkInsertColumn(generator.Addresses(amount), fileAndColumn[(int)id.column]);
            }
        }

        static void GenerateDates(int[] fileAndColumn)
        {
            string stringAmount = "";
            bool choiceConfirmed = false;
            bool triedNonNumeric = false;

            while (!choiceConfirmed)
            {
                Console.Clear();
                Console.WriteLine("CHOOSE AMMOUNT\n\n");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                Console.Write($"\nnumber of entries: ");

                stringAmount = Console.ReadLine();

                if (HandleNumericInput(stringAmount)) continue;

                Console.Clear();

                Console.WriteLine($"Proceed with {stringAmount}?");

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
            }

            int amount = int.Parse(stringAmount);

            Generate generate = new Generate();
            csvList[(int)id.file].ModIndex(0, "start_date", 0);
            csvList[(int)id.file].ModIndex(0, "end_date", 1);
            csvList[(int)id.file].AddBlock(generate.Dates(amount));
        }

        static void constructCompositeManualEntries()
        {
            string stringAmmount = "";
            int[] entriesInFile = new int[2];
            int tempInt = 0;
            int iter = 0;
            bool choiceConfirmed = false;
            bool triedNonNumeric = false;
            bool outOfRange = false;
            while (!choiceConfirmed | iter < 2)
            {
                choiceConfirmed = false;
                Console.Clear();
                Console.WriteLine($"\nSet Number of Entries for Theoretical Source {iter + 1}");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                Console.Write("\nNumber of Entries:");
                string stringEntries = Console.ReadLine() ?? " ";

                triedNonNumeric = HandleNumericInput(stringEntries);

                if (triedNonNumeric) continue;
                tempInt = int.Parse(stringEntries);
                Console.Clear();
                Console.WriteLine($"Proceed With {tempInt} Entries?");

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
                if (choiceConfirmed)
                {
                    entriesInFile[iter] = tempInt;
                    iter++;
                }
            }
            choiceConfirmed = false;
            while (!choiceConfirmed)
            {
                Console.Clear();
                Console.WriteLine("CHOOSE AMMOUNT\n\n");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                if (outOfRange)
                {
                    Console.WriteLine("\nERROR: Out of range!\n\nPlease try again!");
                }

                Console.Write($"\nnumber of entries: ");

                stringAmmount = Console.ReadLine();

                if (HandleNumericInput(stringAmmount)) continue;

                Console.Clear();

                Console.WriteLine($"Proceed with {stringAmmount}?");

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
            }
            choiceConfirmed = false;
            bool triedNothing = false;
            string fileName = "";
            while (!choiceConfirmed)
            {
                Console.Clear();
                Console.WriteLine("NAME FILE\n\n");

                if (triedNothing)
                {
                    Console.WriteLine("ERROR: Filename cannot be empty");
                }

                Console.Write($"\nfile name: ");

                string tempfileName = Console.ReadLine() ?? "";

                triedNothing = tempfileName == "";

                if (triedNothing) continue;

                Console.Clear();

                Console.WriteLine($"Proceed with {tempfileName}?");

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
                if (choiceConfirmed)
                {
                    fileName = tempfileName;
                }
            }

            Csv Composite = new Csv(fileName, 4);
            int amount = int.Parse(stringAmmount);


            Random rand1 = new Random();
            Random rand2 = new Random();
            string[][] temp = new string[amount][];
            int j = 0;
            Generate generate = new Generate();
            Parallel.ForEach<string[]>(temp.Cast<string[]>(), row =>
            {
                string[][] dates = generate.Dates(1);
                row = new string[Composite.Columns];
                row[0] = rand1.Next(1, entriesInFile[0]).ToString();
                row[1] = rand2.Next(1, entriesInFile[1]).ToString();
                row[2] = dates[0][0];
                row[3] = dates[0][1];
                lock (temp)
                {
                    temp[j] = row;
                    j++;
                }
            });

            Composite.InsertRow(new string[] { $"P_id", $"C_id", $"start_date", "end_date" }, 0);

            Composite.AddBlock(temp);

            csvList[insertCounter] = Composite;
        }

        // construct a file with foreign key numbers matching the number of entries in two other files
        static void constructComposite()
        {
            bool choiceConfirmed = false;
            bool triedNonNumeric = false;
            bool outOfRange = false;
            string stringAmmount = ""; // initialize string
            ConsoleKeyInfo key = new();
            int[] filesandcolumns = new int[2]; // used to pass on which file and which column in said file that should be adressed
            int fileIndex = 0;
            int iter = 0;
            while (!choiceConfirmed | iter < 2 )// in order to choose two separate files, the selection is run twice
            {
                choiceConfirmed = false;
                Console.Clear();
                Console.WriteLine($"\nPICK FILE {iter + 1}");

                int i = 1;
                foreach (Csv csv in csvList) // display all CSVs that have been created in the current session (beware your RAM!)
                {
                    Console.Write($"{i}) {csv.fileName}  ");
                    i++;
                }
                Console.Write(">Press Key<");
                key = Console.ReadKey();

                if (!char.IsNumber(key.KeyChar))
                {
                    continue;
                }
                fileIndex = int.Parse(key.KeyChar.ToString());
                Console.Clear();
                Console.WriteLine($"Modify {csvList[int.Parse(key.KeyChar.ToString()) - 1].fileName}?");

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
                fileIndex = int.Parse(key.KeyChar.ToString());


                filesandcolumns[iter] = fileIndex - 1;
                iter++;
            }
            choiceConfirmed = false;
            while (!choiceConfirmed)
            {
                Console.Clear();
                Console.WriteLine("CHOOSE AMMOUNT\n\n");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                if (outOfRange)
                {
                    Console.WriteLine("\nERROR: Out of range!\n\nPlease try again!");
                }

                Console.WriteLine($"\nType corresponding number: ");

                stringAmmount = Console.ReadLine();

                if (HandleNumericInput(stringAmmount)) continue;

                Console.Clear();

                Console.WriteLine($"Proceed with {stringAmmount}?");

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
            }

            Csv Composite = new Csv("Rental", 3);

            int indecies1 = csvList[filesandcolumns[0]].Rows.Count - 1;
            int indecies2 = csvList[filesandcolumns[1]].Rows.Count - 1;
            int amount = int.Parse(stringAmmount);


            Random rand1 = new Random();
            Random rand2 = new Random();
            string[][] temp = new string[amount][];
            int j = 0;
            Generate generate = new Generate();
            Parallel.ForEach<string[]>(temp.Cast<string[]>(), row =>
            {
                row = new string[Composite.Columns];
                row[0] = rand1.Next(0, indecies1).ToString();
                row[1] = rand2.Next(0, indecies2).ToString();
                row[2] = generate.prices(1)[0];
                lock (temp)
                {
                    temp[j] = row;
                    j++;
                }
            });

            Composite.InsertRow(new string[] { $"{csvList[filesandcolumns[0]].fileName.First().ToString()}_id", $"{csvList[filesandcolumns[1]].fileName.First().ToString()}_id", $"Price" }, 0);

            Composite.AddBlock(temp);

            csvList[8] = Composite;
        }

        static async void PushToFiles()
        {
            ConcurrentBag<CsvIOTranslator> translators = new ConcurrentBag<CsvIOTranslator>();
            uint[] validIndicies = new uint[9];
            int numberofvalid = 0;
            for (uint i = 0; i < 9; i++)
            {
                switch (csvList[i].Columns)
                {
                    case 0:
                        continue;
                    default:
                        validIndicies[numberofvalid] = i;
                        numberofvalid++;
                        continue;
                }
            }

            for (uint i = 0; i < numberofvalid; i++)
            {
                Csv csv = csvList[validIndicies[i]];
                if (csv.Rows[0].Length == 2)
                {
                    CsvIOTranslator temp = new CsvIOTranslator(csv, CSVOutputPath);
                    temp.BulkBuildString3WideAsync(true);
                    translators.Add(temp);
                }
                else if (csv.Rows[0].Length == 3)
                {
                    CsvIOTranslator temp = new CsvIOTranslator(csv, CSVOutputPath);
                    temp.BulkBuildString4WideAsync(true);
                    translators.Add(temp);
                }
                else if (csv.Columns >= 4)
                {
                    CsvIOTranslator temp = new CsvIOTranslator(csv, CSVOutputPath);
                    temp.BulkBuildString5WideAsync(true);
                    translators.Add(temp);
                }
                else
                {
                    CsvIOTranslator temp = new CsvIOTranslator(csv, CSVOutputPath);
                    temp.BuildString(true);
                    translators.Add(temp);
                }
            }

            Parallel.ForEach(translators.ToArray().Cast<CsvIOTranslator>(), async writer =>
            {
                await writer.PushToFileAsync();
            });

        }

        static void GenerateSSN(int[] fileAndColumn)
        {
            bool choiceConfirmed = false;

            choiceConfirmed = false;
            bool triedNonNumeric = false;
            string stringAmmount = "";
            while (!choiceConfirmed)
            {
                //Get Ammount
                Console.Clear();
                Console.WriteLine("CHOOSE AMMOUNT\n");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                Console.WriteLine($"\nfile name: {csvList[fileAndColumn[(int)id.file]].fileName}");


                Console.Write("Number of Social Security Numbers to generate:");
                stringAmmount = Console.ReadLine();

                if (HandleNumericInput(stringAmmount)) continue;

                while (!choiceConfirmed && !triedNonNumeric)
                {
                    Console.WriteLine($"Is {stringAmmount} correct?");

                    Console.WriteLine("\n[ENTER] Yes");
                    Console.WriteLine("[ESC] No (retype)\n\n");

                    Console.Write(">Press Key<");
                    choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
                }
            }
            int amount = int.Parse(stringAmmount);

            Generate generator = new();

            // check if file already has entries to detirmine correct method for insertion of data.
            if (csvList[fileAndColumn[(int)id.file]].Rows.Count <= 1)
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "SSN", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkAddColumn(generator.SSN(amount), fileAndColumn[(int)id.column]);

            }
            // if file already has entries, use method for inserting into already existing spaces.
            else
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "SSN", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkInsertColumn(generator.SSN(amount), fileAndColumn[(int)id.column]);
            }
        }

        static void GenerateLicencePlates(int[] fileAndColumn)
        {
            bool choiceConfirmed = false;

            choiceConfirmed = false;
            bool triedNonNumeric = false;
            string fileName = "";
            string stringAmmount = "";
            while (!choiceConfirmed)
            {
                //Get Ammount
                Console.Clear();
                Console.WriteLine("CHOOSE AMMOUNT\n");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                Console.WriteLine($"\nfile name: {csvList[fileAndColumn[(int)id.file]].fileName}");


                Console.Write("Number of licence plates to generate: ");
                stringAmmount = Console.ReadLine();

                if (HandleNumericInput(stringAmmount)) continue;

                while (!choiceConfirmed && !triedNonNumeric)
                {
                    Console.WriteLine($"Is {stringAmmount} correct?");

                    Console.WriteLine("\n[ENTER] Yes");
                    Console.WriteLine("[ESC] No (retype)\n\n");

                    Console.Write(">Press Key<");
                    choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
                }
            }
            int amount = int.Parse(stringAmmount);

            Generate generator = new();
            // check if file already has entries to detirmine correct method for insertion of data.
            if (csvList[fileAndColumn[(int)id.file]].Rows.Count <= 1)
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "Licence Plates", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkAddColumn(generator.RegNumbers(amount), fileAndColumn[(int)id.column]);

            }
            // if file already has entries, use method for inserting into already existing spaces.
            else
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "Licence Plates", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkInsertColumn(generator.RegNumbers(amount), fileAndColumn[(int)id.column]);
            }
        }
        static void GenerateFraudLicencePlates(int[] fileAndColumn)
        {
            bool choiceConfirmed = false;

            choiceConfirmed = false;
            bool triedNonNumeric = false;
            string stringAmmount = ""; // initialize string
            while (!choiceConfirmed)
            {
                //Get Ammount
                Console.Clear();
                Console.WriteLine("CHOOSE AMMOUNT\n");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                Console.WriteLine($"\nfile name: {csvList[fileAndColumn[(int)id.file]].fileName}");


                Console.Write("Number of licence plates to generate: ");
                stringAmmount = Console.ReadLine();

                if (HandleNumericInput(stringAmmount)) continue;

                while (!choiceConfirmed && !triedNonNumeric)
                {
                    Console.WriteLine($"Is {stringAmmount} correct?");

                    Console.WriteLine("\n[ENTER] Yes");
                    Console.WriteLine("[ESC] No (retype)\n\n");

                    Console.Write(">Press Key<");
                    choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
                }
            }
            int amount = int.Parse(stringAmmount);

            Generate generator = new();

            // check if file already has entries to detirmine correct method for insertion of data.
            if (csvList[fileAndColumn[(int)id.file]].Rows.Count <= 1)
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "Licence Plates", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkAddColumn(generator.FraudReg(amount), fileAndColumn[(int)id.column]);

            }
            // if file already has entries, use method for inserting into already existing spaces.
            else
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "Licence Plates", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkInsertColumn(generator.FraudReg(amount), fileAndColumn[(int)id.column]);
            }
        }
        static void GenerateManufacturers(int[] fileAndColumn)
        {
            bool choiceConfirmed = false;

            choiceConfirmed = false;
            bool triedNonNumeric = false;
            string fileName = "";
            string stringAmmount = "";
            while (!choiceConfirmed)
            {
                //Get Ammount
                Console.Clear();
                Console.WriteLine("CHOOSE AMMOUNT\n");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                Console.WriteLine($"\nfile name: {csvList[fileAndColumn[(int)id.file]]}");


                Console.Write("Number of Manufacturers to generate:");
                stringAmmount = Console.ReadLine();

                if (HandleNumericInput(stringAmmount)) continue;

                while (!choiceConfirmed && !triedNonNumeric)
                {
                    Console.WriteLine($"Is {stringAmmount} correct?");

                    Console.WriteLine("\n[ENTER] Yes");
                    Console.WriteLine("[ESC] No (retype)\n\n");

                    Console.Write(">Press Key<");
                    choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);
                }
            }
            int amount = int.Parse(stringAmmount);

            Generate generator = new();
            // check if file already has entries to detirmine correct method for insertion of data.
            if (csvList[fileAndColumn[(int)id.file]].Rows.Count <= 1)
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "Manufacturers", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkAddColumn(generator.Manufacturers(amount, csvValues.Manufacturers), fileAndColumn[(int)id.column]);

            }
            // if file already has entries, use method for inserting into already existing spaces.
            else
            {
                csvList[fileAndColumn[(int)id.file]].ModIndex((int)id.file, "Manufacturers", fileAndColumn[(int)id.column]);
                csvList[fileAndColumn[(int)id.file]].BulkInsertColumn(generator.Manufacturers(amount, csvValues.Manufacturers), fileAndColumn[(int)id.column]);
            }
        }

        static int[] HandleSelection() // handles the selection of a data holder/'file' and column to modify when an option is selected from the starting menu.
        {
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            bool choiceConfirmed = false;
            while (!choiceConfirmed)
            {
                Console.Clear();
                Console.WriteLine("PICK FILE\n\n");

                if (!csvList.Any<Csv>()) // if the csv array is empty, return a new array with '-1'
                {
                    return new[] { -1 };
                }

                int i = 1;
                foreach (Csv csv in csvList) // display all CSVs currently stored in memory
                {
                    Console.Write($"{i}) {csv.fileName}  ");
                    i++;
                }

                Console.Write("\n\n>Press Corresponding Number<");
                key = Console.ReadKey(); // get input from user

                if (!char.IsNumber(key.KeyChar)) // check if any bogus keys have been pressed to avoid crash
                {
                    continue;
                }
                if (int.Parse(key.KeyChar.ToString()) > insertCounter) // check if number recieved exeeds relevant range
                {
                    continue;
                }
                Console.Clear(); // prepare for next menu slide
                Console.WriteLine($"Modify {csvList[int.Parse(key.KeyChar.ToString()) - 1].fileName}?"); // get user confirmation

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);

            }
            choiceConfirmed = false;
            bool triedNonNumeric = false;
            bool outOfRange = false;
            string stringColumn = "";
            int fileIndex = int.Parse(key.KeyChar.ToString()); // for readability and ease of use when coding, the chosen data holder is converted into an integer called 'fileIndex'
            while (!choiceConfirmed) // handle choice of column in the same manner as the selection of the file above
            {
                Console.Clear();
                Console.WriteLine("CHOOSE COLUMN\n\n");


                int i = 1;
                foreach (string? value in csvList[fileIndex - 1].Rows.FirstOrDefault<string[]>(new string[csvList[fileIndex - 1].Columns])) // runs the loop for each column in chosen data holder
                {
                    if (string.IsNullOrEmpty(value)) // if the column is empty/NULL then display to that effect without throwing an exception.
                    {
                        Console.Write($"{i}) [EMPTY]  ");
                        i++;
                        continue;
                    }
                    Console.Write($"{i}) {value}  ");
                    i++;
                }

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!"); // tell user to behave
                }

                if (outOfRange)
                {
                    Console.WriteLine("\nERROR: Out of range!\n\nPlease try again!"); // no, like seriously, stop
                }

                Console.WriteLine($"\nType corresponding number: "); // prompt user for input

                stringColumn = Console.ReadLine(); // get user input
                triedNonNumeric = HandleNumericInput(stringColumn);
                if (triedNonNumeric) continue; // check if input is actually a number. if not, skip to next iteration of loop

                if (int.Parse(stringColumn) > csvList[fileIndex - 1].Columns) { outOfRange = true; continue; }

                Console.Clear();

                Console.WriteLine($"Modify column {stringColumn}"); // get user confirmation

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);


            }


            return new[] { fileIndex - 1, int.Parse(stringColumn) - 1 }; // return int array with data holder index in first space, and CSV column index in second space.
        }

        // Handles the confirmation of a choice. Returns false if escape or any key other than 'ENTER' is pressed. Returns true if ENTER is pressed.
        static bool HandleConfirmation(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.Enter:
                    return true;
                case ConsoleKey.Escape:
                    return false;
                default:
                    return false;
            }

        }

        // Checks if the input string recieved is a number or not. if it is, return false. If not, return true.
        static bool HandleNumericInput(string input)
        {
            if (!string.IsNullOrEmpty(input))
                foreach (char c in input.ToCharArray())
                {
                    if (!char.IsNumber(c))
                    {
                        return true;
                    }
                }
            return false;
        }

        static void GenerateNames(int[] fileAndColumn)
        {
            bool choiceConfirmed = false;

            choiceConfirmed = false;
            bool triedNonNumeric = false;
            string stringAmmount = "";
            while (!choiceConfirmed)
            {
                //Get Ammount
                Console.Clear();
                Console.WriteLine("CHOOSE AMMOUNT\n");

                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                Console.WriteLine($"\nfile name: {csvList[fileAndColumn[(int)id.file]].fileName}");


                Console.Write("Number of names to generate:");
                stringAmmount = Console.ReadLine();

                if (HandleNumericInput(stringAmmount)) continue;

                while (!choiceConfirmed && !triedNonNumeric)
                {
                    Console.WriteLine($"Is {stringAmmount} correct?");

                    Console.WriteLine("\n[ENTER] Yes");
                    Console.WriteLine("[ESC] No (retype)\n\n");

                    Console.Write(">Press Key<");
                    choiceConfirmed = HandleConfirmation(Console.ReadKey().Key);


                }
            }
            int amount = int.Parse(stringAmmount);

            Generate generator = new();

            if (csvList[fileAndColumn[0]].Rows.Count <= 1)
            {
                csvList[fileAndColumn[0]].ModIndex(0, "Names", fileAndColumn[1]);
                csvList[fileAndColumn[0]].BulkAddColumn(generator.Names(amount, csvValues.Names.FirstNames, csvValues.Names.LastNames), fileAndColumn[1]);

            }
            else
            {
                csvList[fileAndColumn[0]].ModIndex(0, "Names", fileAndColumn[1]);
                csvList[fileAndColumn[0]].BulkAddColumn(generator.Names(amount, csvValues.Names.FirstNames, csvValues.Names.LastNames), fileAndColumn[1]);
            }
        }

        static void CreateDataHolder()
        {
            bool triedEmptyInput = false;
            bool choiceConfirmed = false;
            string fileName = "";
            while (!choiceConfirmed)
            {
                Console.Clear();
                Console.WriteLine("MAKE NEW DATA HOLDER");

                if (triedEmptyInput)
                {
                    Console.WriteLine("EMPTY INPUT DETECTED!\n\nPlease try again!");
                }

                Console.Write("\nInput File Name: ");
                fileName = Console.ReadLine();


                if (fileName == null)
                {
                    continue;
                }

                Console.WriteLine($"Is {fileName} correct?");

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                Console.Write(">Press Key<");
                ConsoleKeyInfo key = Console.ReadKey();

                choiceConfirmed = HandleConfirmation(key.Key);
            }
            bool triedNonNumeric = false;
            choiceConfirmed = false;
            string stringAmmount = "";
            while (!choiceConfirmed)
            {
                Console.Clear();
                Console.WriteLine("COLUMNS");

                Console.WriteLine("\n");

                Console.Write("\nNumber of Columns: ");
                stringAmmount = Console.ReadLine();

                Console.WriteLine($"\nfile name: {fileName}");
                if (triedNonNumeric)
                {
                    Console.WriteLine("\nNON NUMERIC VALUE DETECTED!\nERROR: CANNOT PARSE!\n\nPlease try again!");
                }

                if (HandleNumericInput(stringAmmount)) continue;
                triedNonNumeric = true;

                Console.WriteLine($"Is {stringAmmount} correct?");

                Console.WriteLine("\n[ENTER] Yes");
                Console.WriteLine("[ESC] No (retype)\n\n");

                Console.Write(">Press Key<");
                ConsoleKeyInfo key = Console.ReadKey();

                choiceConfirmed = HandleConfirmation(key.Key);
            }



            int amount = int.Parse(stringAmmount);
            Csv csv = new Csv(fileName, amount);

            csv.AddRow(new string[amount]);

            csvList[insertCounter] = csv;
        }


    }

    struct CsvIOTranslator // struct that handles writing to disk from the data stored in a CSV data holder.
    {
        StringBuilder StringData; // used to make sure that the memory limit of a C# standard string is not exceeded
        private string[][] FileData;
        readonly string fileName;
        readonly string DirectoryPath;
        private string tempPath;
        private int columns;
        public string FullPath { get; private set; }
        public CsvIOTranslator(Csv csvData, string folderPath) // constructor
        {
            this.FileData = csvData.Rows.ToArray();
            this.DirectoryPath = folderPath;
            this.fileName = csvData.fileName;
            this.columns = csvData.Columns;
            this.FullPath = Path.Combine(folderPath, csvData.fileName);
            this.tempPath = Path.Combine("F:\\DBcsv\\Temp", csvData.fileName); // used for debug purposes
            StringData = new StringBuilder();
        }


        public void BuildString(bool createindex = false)
        {
            string[] flat = FileData.SelectMany(a => a).ToArray(); // flattens the array array into a single dimensional array in order to fetch an accurate byte-size
            StringData = new StringBuilder(Buffer.ByteLength(flat.ToString().ToCharArray()) * 2); // fetch byte-size
            flat = null; // dump the stored data, should probably have been done with scope brackets.
            string line;
            int columns = this.FileData[0].Length;
            int j = 0;

            switch (createindex)
            {
                case true:

                    line = $"id,";
                    for (int i = 0; i < columns; i++)
                    {
                        line += $"{FileData[0][i]}"
;                   }
                    line += "\n";
                    StringData.Append(line);

                    for (int i = 1; i < this.FileData.Length; i++)
                    {
                        line = $"{i},";
                        for (j = 0; j < columns - 2; j++)
                        {
                            line += $"{FileData[i][j]},";
                        }
                        line += $"{FileData[i][columns - 1]}\n";
                        StringData.Append(line);
                    }
                    break;
                case false:
                    for (int i = 0; i < this.FileData.Length; i++)
                    {
                        line = "";
                        for (j = 0; j < columns - 2; j++)
                        {
                            line += $"{FileData[i][j]},";
                        }
                        line += $"{FileData[i][columns - 1]}\n";
                        StringData.Append(line);
                    }
                    break;
            }

        }

        public void BulkBuildString3WideAsync(bool makeID = false)
        {
            string[] flat = FileData.SelectMany(a => a).ToArray(); // flattens the array array into a single dimensional array in order to fetch an accurate byte-size
            StringData = new StringBuilder(Buffer.ByteLength(flat.ToString().ToCharArray()) * 2); // fetch byte-size
            flat = null; // dump the stored data, should probably have been done with scope brackets.
            string line;
            switch (makeID)
            {
                case true:
                    int id = 0;
                    line = $"id,{FileData[0][0]},{FileData[0][1]}\n";
                    StringData.Append(line);
                    for (id = 1; id < FileData.Length; id++)
                    {
                        line = $"{id},{FileData[id][0]},{FileData[id][1]}\n";
                        StringData.Append(line);
                    }
                    break;
                case false:
                    for (int i = 0; i < FileData.Length; i++)
                    {
                        line = $"{FileData[i][0]},{FileData[i][1]}\n";
                        StringData.Append(line);
                    }
                    break;
            }
        }

        public void BulkBuildString4WideAsync(bool makeID = false)
        {
            string[] flat = FileData.SelectMany(a => a).ToArray(); // flattens the array array into a single dimensional array in order to fetch an accurate byte-size
            StringData = new StringBuilder(Buffer.ByteLength(flat.ToString().ToCharArray()) * 2); // fetch byte-size
            flat = null; // dump the stored data, should probably have been done with scope brackets.
            string line;
            switch (makeID)
            {
                case true:
                    int id = 0;
                    line = $"id,{FileData[id][0]},{FileData[id][1]},{FileData[id][2]}\n";
                    StringData.Append(line);
                    for (id = 1; id < FileData.Length; id++)
                    {
                        line = $"{id + 1},{FileData[id][0]},{FileData[id][1]},{FileData[id][2]}\n";
                        StringData.Append(line);
                    }
                    break;
                case false:

                    for (id = 0; id < FileData.Length; id++)
                    {
                        line = $"{FileData[id][0]},{FileData[id][1]},{FileData[id][2]}\n";
                        StringData.Append(line);
                    }
                    break;
            }

        }

        public void BulkBuildString5WideAsync(bool makeID = false)
        {
            string[] flat = FileData.SelectMany(a => a).ToArray(); // flattens the array array into a single dimensional array in order to fetch an accurate byte-size
            StringData = new StringBuilder(Buffer.ByteLength(flat.ToString().ToCharArray())); // fetch byte-size
            flat = null; // dump the stored data, should probably have been done with scope brackets.
            string line;
            switch (makeID)
            {
                case true:
                    int id = 0;
                    line = $"id,{FileData[id][0]},{FileData[id][1]},{FileData[id][2]},{FileData[id][3]}\n";
                    StringData.Append(line);
                    for (id = 1; id < FileData.Length; id++)
                    {
                        line = $"{id},{FileData[id][0]},{FileData[id][1]},{FileData[id][2]},{FileData[id][3]}\n";
                        StringData.Append(line);
                    }
                    break;
                case false:

                    for (id = 0; id < FileData.Length; id++)
                    {
                        line = $"{FileData[id][0]},{FileData[id][1]},{FileData[id][2]},{FileData[id][3]}\n";
                        StringData.Append(line);
                    }
                    break;
            }

        }

        public void DropData()
        {
            StringData.Clear();
        }

        public readonly async Task PushToFileAsync()
        {
            //Stream stream = null;
            //try
            //{
            //    byte[] buffer = Encoding.UTF8.GetBytes(StringData.ToString());
            //    stream = new LargeBufferStream(File.Create(tempPath), buffer.Length);
            //    int chunkSize = 2048; // adjust the chunk size as needed
            //    for (int i = 0; i < buffer.Length; i += chunkSize)
            //    {
            //        int chunkLength = Math.Min(chunkSize, buffer.Length - i);
            //        byte[] chunk = new byte[chunkLength];
            //        Buffer.BlockCopy(buffer, i, chunk, 0, chunkLength);
            //        try
            //        {
            //            //await streamData.WriteAsync(chunk, 0, chunkLength);
            //            await stream.WriteAsync(chunk, 0, chunk.Length);
            //            stream.Flush();
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.ToString());
            //            Console.ReadKey();
            //        }
            //    }
            //}
            //finally
            //{
            //    if (stream != null)
            //    {
            //        await stream.FlushAsync();
            //        await stream.DisposeAsync();
            //    }
            //}
            await File.WriteAllTextAsync(FullPath, StringData.ToString());
        }

        private readonly bool CheckFileExists()
        {
            string[] files = Directory.GetFiles(DirectoryPath);

            int trimIndex = DirectoryPath.LastIndexOf(Path.PathSeparator);
            foreach (string filePath in files)
            {
                if (fileName == filePath.Substring(trimIndex))
                { return true; }
            }
            return false;
        }

    }

}
public class LargeBufferStream : Stream
{
    private readonly Stream _innerStream;
    private readonly int _bufferSize;

    public LargeBufferStream(Stream innerStream, int bufferSize)
    {
        _innerStream = innerStream;
        _bufferSize = bufferSize;
    }

    public override bool CanRead => _innerStream.CanRead;

    public override bool CanWrite => _innerStream.CanWrite;

    public override bool CanSeek => _innerStream.CanSeek;

    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    public override void SetLength(long value)
    {
        _innerStream.SetLength(value);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = 0;
        byte[] largeBuffer = new byte[_bufferSize];
        while (bytesRead < count)
        {
            int largeBufferLength = Math.Min((count - bytesRead), _bufferSize);
            int largeBufferRead = _innerStream.Read(largeBuffer, 0, largeBufferLength);
            Array.Copy(largeBuffer, 0, buffer, offset + bytesRead, largeBufferRead);
            bytesRead += largeBufferRead;
        }
        return bytesRead;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        byte[] largeBuffer = new byte[_bufferSize];
        while (count > 0)
        {
            int largeBufferLength = Math.Min(count, _bufferSize);
            Array.Copy(buffer, offset, largeBuffer, 0, largeBufferLength);
            _innerStream.Write(largeBuffer, 0, largeBufferLength);
            offset += largeBufferLength;
            count -= largeBufferLength;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _innerStream.Seek(offset, origin);
    }

    public override void Flush()
    {
        _innerStream?.Flush();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _innerStream.Dispose();
        }
        base.Dispose(disposing);
    }
}
struct Csv // struct that holds all of the information, methods and data necissary to create a CSV file
{
    public int Columns { get; }
    public string fileName { get; private set; } //file name without extension

    public List<string[]> Rows { get; private set; } // a dynamic list of string arrays that holds all of the data supposed to be in the final file
    public Csv(string fileName, int columns) // constructor
    {
        this.Columns = columns;
        this.fileName = fileName + ".csv";
        this.Rows = new List<string[]>(); // initialize new list for CSV data
    }

    public readonly void Add(string value, int column) // construct singular new row at end of list with data in a certain column
    {

        string[] row = new string[Columns];
        row[column] = value;
        Rows.Add(row);
    }

    public readonly void AddRow(string[] values) // add a new row to end of list with data.
    {
        Rows.Add(values);
    }

    public readonly void AddBlock(string[][] block) // add a chunk of values in bulk from an array of arrays matching the width of CSV file
    {
        Rows.AddRange(block.AsEnumerable());
    }

    public readonly void BulkAddColumn(string[] values, int column, int skipTo = 0) // insert an arbitrary number of values into a single column from an array
    {
        for (int i = skipTo; i < values.Length; i++)
        {
            string[] temp = new string[Columns];
            temp[column] = values[i];
            Rows.Add(temp);
        }
    }

    public readonly void Insert(int index, string value, int column) // insert a value into a (preferably empty) slot in an already existing row
    {
        string[] row = new string[Columns];
        row[column] = value;
        Rows.Insert(index, row);
    }

    public readonly void ModIndex(int index, string value, int column) // modify an already existing index/value/slot in a given row
    {
        Rows[index][column] = value;
    }

    public readonly void InsertRow(string[] values, int index) // inserts a whole new row at a give index with values.
    {
        Rows.Insert(index, values);
    }

    public readonly void InsertBlock(string[][] block, int index) // inserts a block of data starting at a given index.
    {
        for (int i = 0; i < Rows.Count; i++)
        {                           //stupid cast
            Rows.InsertRange(index, (IEnumerable<string[]>)block[i].AsEnumerable());
        }
    }

    public readonly void BulkInsertColumn(string[] values, int column) // inserts an arbitrary number of new rows with data in a given column starting at an index
    {
        int endingindex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (i + 1 < Rows.Count)
            {
                Rows[i + 1]?.SetValue(values[i], column);
            }
            else
            {
                endingindex = i;
                break;
            }
            endingindex = i; // visual studio gets mad at me if I remove this :(
        }

        if (endingindex != values.Length - 1)
            BulkAddColumn(values, column, endingindex);
    }
}

// class used to separate the generation algorithms from the rest of the program for readability as well as to make it easier to write.
class Generate
{
    // generate 'amount' number of unique SSNs
    public string[] FraudSSN(int amount)
    {
        string[] output = new string[amount];
        char[] chars;
        Random rand = new Random();
        for (int i = 0; i < amount; i++)
        {
            do
            {
#pragma warning disable CS8601 // Possible null reference assignment.
            // Make a new char array to store SSN, not integer because it needs to return string.
            chars = new char[] {
                char.Parse(rand.Next(9).ToString()),
                char.Parse(rand.Next(9).ToString()),
                char.Parse(rand.Next(9).ToString()),
                char.Parse(rand.Next(9).ToString()),
                char.Parse(rand.Next(9).ToString()),
                char.Parse(rand.Next(9).ToString()),
                char.Parse(rand.Next(9).ToString()),
                char.Parse(rand.Next(9).ToString()),
                char.Parse(rand.Next(9).ToString())};
#pragma warning restore CS8601 // Possible null reference assignment.
            } while (output.Contains(new string(chars))); // run again if the SSN already exists
            output[i] = new string(chars) ?? "NULL"; // store unique SSN in the output string array
        }
        return output; // return array with SSNs
    }

    // Generate names based on the first and last names found in a source CSV file.
    public string[] Names(int amount, string[] firstNames, string[] lastNames)
    {
        Random firstNameRand = new Random();
        Random lastNameRand = new Random();

        List<string> names = new List<string>();

        for (int i = 0; i < amount; i++)
        {
            names.Add(firstNames[firstNameRand.Next(0, firstNames.Length)] + " " + lastNames[lastNameRand.Next(0, lastNames.Length)]);
        }

        return names.ToArray();
    }

    //generate an array of manufacturers based on the ones available in the source CSV file
    public string[] Manufacturers(int amount, string[] manufacturers)
    {
        Random random = new Random();

        List<string> manufacturerList = new List<string>();

        for (int i = 0; i < amount; i++)
        {
            manufacturerList.Add(manufacturers[random.Next(0, manufacturers.Length)]);
        }

        return manufacturerList.ToArray();
    }

    // generate an array of prices ranging between 1k and 20k
    public string[] prices(int amount)
    {
        Random random = new Random();
        List<string> prices = new List<string>();

        for (int i = 0; i <= amount; i++)
        {
            prices.Add((random.Next(1, 20) * 1000).ToString());
        }

        return prices.ToArray();
    }

    // generate a chunk of dates where the dates in the first position are always smaller than the dates in the second position.
    public string[][] Dates(int amount)
    {
        string[][] names = new string[amount][];
        DateTime date = new DateTime(2024, 1, 1);
        Random generator = new Random();
        int range = (new DateTime(2028, 12, 31) - date).Days;

        for (int i = 0; i < amount; i++)
        {
            names[i] = new string[2];
            int days1 = 0;
            int days2 = 0;
            do
            {
                days1 = generator.Next(range);
                days2 = generator.Next(range);
            } while (days2 == days1);

            bool state = days1 > days2;

            switch (state) // make sure that the smaller date is always put in the first position and vice versa.
            {
                case false:
                    names[i][0] = new DateTime(date.Year, date.Month, date.Day).AddDays(days1).ToString("yyyy-MM-dd");
                    names[i][1] = new DateTime(date.Year, date.Month, date.Day).AddDays(days2).ToString("yyyy-MM-dd");
                    break;
                case true:
                    names[i][0] = new DateTime(date.Year, date.Month, date.Day).AddDays(days2).ToString("yyyy-MM-dd");
                    names[i][1] = new DateTime(date.Year, date.Month, date.Day).AddDays(days1).ToString("yyyy-MM-dd");
                    break;
            }
        }
        return names;
    }

    // generate a street name and number using the format [Capital letter A-Z]Street[Number 0000-2000]
    public string[] Addresses(int amount)
    {
        string[] addresses = new string[amount];
        Random rand = new Random();
        char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        for (int i = 0; i < amount; i++)
        {
            addresses[i] = $"{letters[rand.Next(25)]}Street" + rand.Next(2000).ToString("D4");
        }
        return addresses;
    }

    //generate a social security number / person number
    public string[] SSN(int amount) // this code was at least partially generated by AI, hence the redundant attempt at using parallelization, which would not have worked because of assignment overlap.
    {
        var uniquePersonNumbers = new ConcurrentBag<string>(); // concurrent bag to store valid generated numbers, could just have been a list
        int targetCount = amount; // number of values to generate

        // could've just been a normal for loop for generating unique SSNs
        Parallel.For(0, targetCount, i =>
        {
            string personNumber;
            do
            {
                personNumber = GenerateSwedishPersonNumber();
            } while (uniquePersonNumbers.Contains(personNumber)); // if the value already exists, try again
            uniquePersonNumbers.Add(personNumber);
        });

        return uniquePersonNumbers.ToArray();
    }

    // generate a "licence plate" number.
    public string[] FraudReg(int amount) // this just generates a 6 character long number to better fit the desired data. So it ended up not using the actual registration number generator.
    {
        string[] nums = new string[amount];
        Random rand = new Random();
        for (int i = 0; i < amount; i++)
        {
            nums[i] = rand.Next().ToString("D6");
        }
        return nums;
    }


    // The generation below (for the ID number) started with an AI but was then modified as was necessary by me
    // The same goes for the luhn checksum however that worked from the beginning and is provided as is.
    private string GenerateSwedishPersonNumber()
    {
        Random random = new Random();
        int year = random.Next(1900, 2025); // generate year between 1900 and 2024
        int month = random.Next(1, 13); // generate month between 1 and 12
        int day = random.Next(1, DateTime.DaysInMonth(year, month) + 1); // generate day based on year and month

        string datePart = $"{year % 100:D2}{month:D2}{day:D2}";

        int birthNumber = random.Next(0, 1000); // generate birthnumber between 3 random numbers
        string birthNumberPart = $"{birthNumber:D3}";

        string partialPersonNumber = datePart + birthNumberPart; // construct all but last number of ID
        int checksum = CalculateLuhnChecksum(partialPersonNumber); // generate last number of ID number

        return $"{datePart}{birthNumberPart}{checksum}"; // return final number
    }
    private int CalculateLuhnChecksum(string number)
    {
        int sum = 0;
        bool alternate = false;

        for (int i = number.Length - 1; i >= 0; i--)
        {
            int n = int.Parse(number[i].ToString());

            if (alternate)
            {
                n *= 2;
                if (n > 9)
                {
                    n -= 9;
                }
            }

            sum += n;
            alternate = !alternate;
        }

        return (10 - (sum % 10)) % 10;
    }

    // the following (methods RegNumbers, GenerateSwedishRegNumber) is fully AI generated, however it ended up not getting used but is left here despite this since they do still function.
    public string[] RegNumbers(int amount)
    {
        var uniqueRegNumbers = new ConcurrentBag<string>();
        int targetCount = amount; // number of values to generate
        string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string numbers = "0123456789";


        Parallel.For(0, targetCount, i =>
        {
            string regNumber;
            do
            {
                regNumber = GenerateSwedishRegNumber(letters, numbers);
            } while (uniqueRegNumbers.Contains(regNumber));

            uniqueRegNumbers.Add(regNumber);
        });

        return uniqueRegNumbers.ToArray();
    }

    private string GenerateSwedishRegNumber(string letters, string numbers)
    {

        byte[] letterBytes = new byte[3];
        byte[] numberBytes = new byte[3];

        letterBytes = RandomNumberGenerator.GetBytes(3);
        numberBytes = RandomNumberGenerator.GetBytes(3);

        char letter1 = letters[letterBytes[0] % letters.Length];
        char letter2 = letters[letterBytes[1] % letters.Length];
        char letter3 = letters[letterBytes[2] % letters.Length];

        char number1 = numbers[numberBytes[0] % numbers.Length];
        char number2 = numbers[numberBytes[1] % numbers.Length];
        char number3 = numbers[numberBytes[2] % numbers.Length];

        return $"{letter1}{letter2}{letter3} {number1}{number2}{number3}";
    }
}

public struct CsvValues // struct to fetch values from source CSV files, you can add whatever you want to here I guess...
{

    public NameStruct Names;
    public string[] Manufacturers { get; set; }

    readonly string FilePath;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public CsvValues(string filePath) //constructor
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        this.FilePath = filePath;
        GetCsvValues(FilePath);
    }

    private void GetCsvValues(string path)
    {
        // Läs in CSV-filerna
        string[] filePaths;
        if (Directory.Exists(path))
        {
            filePaths = Directory.GetFiles(path);
        }
        else { return; }

        foreach (string filePath in filePaths)
        {
            switch (Path.GetFileName(filePath.AsSpan()).ToString().ToLower())
            {
                case "names.csv":
                    NameCase(filePath);
                    continue;
                case "manufacturers.csv":
                    ManufaturerCase(filePath);
                    continue;
                default:
                    Console.WriteLine("Error: File not in Switch Case!");
                    continue;
            }
        }
    }

    //Method that fetches names from the CSV file named "names.csv"
    private void NameCase(string path)
    {
        string[] linesTemp = File.ReadAllLines(path);
        string[] lines = new string[linesTemp.Length - 1];

        lines = linesTemp.Skip(1).ToArray();
        List<string> firstNames = new List<string>();
        List<string> lastNames = new List<string>();

        foreach (string line in lines)
        {
            var parts = line.Split(',');
            lastNames.Add(parts[0]);
            firstNames.Add(parts[1]);
            firstNames.Add(parts[2]);
        }

        this.Names.FirstNames = firstNames.ToArray();
        this.Names.LastNames = lastNames.ToArray();
    }

    //Method that fetches the names of car manufacturers from the file named "manufacturers.csv"
    private void ManufaturerCase(string path)
    {
        string[] linesTemp = File.ReadAllLines(path);
        string[] lines = new string[linesTemp.Length - 1];

        lines = linesTemp.Skip(1).ToArray();
        List<string> Manufacturers = new List<string>();
        foreach (string line in lines)
        {
            var parts = line.Split(',');
            Manufacturers.Add(parts[0]);
        }

        this.Manufacturers = Manufacturers.ToArray();
    }

}
public struct NameStruct // struct to hold names in a single piece of memory for tidyness.
{
    public string[] FirstNames { get; set; }
    public string[] LastNames { get; set; }
}

