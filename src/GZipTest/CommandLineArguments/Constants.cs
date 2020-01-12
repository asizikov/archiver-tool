namespace GZipTest.CommandLineArguments
{
    public static class Constants
    {
        public const string Help =
            @"to compress a file call this application with the following parameters:
       GZipTest.exe compress path/to/input_file path/to/outputfile
  to decompress a file call this application with the following parameters:
       GZipTest.exe decompress path/to/input_file path/to/outputfile";

        public static class ValidationErrors
        {
            public const string TooFewArguments = "To few command line arguments";

            public const string UnknownCommand =
                "unknown command, please use 'compress' or 'decompress' as a first argument";

            public const string InvalidInputFile = "Invalid input file";
            public const string InvalidOutputFile = "Invalid output file";
        }
    }
}