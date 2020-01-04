using System;
using System.IO;

namespace GZipTest.CommandLineArguments
{
    public sealed class CommandLineValidator : ICommandLineValidator
    {
        public ValidationResult Validate(string[] args)
        {
            var result = new ValidationResult();

            if (args.Length < 3)
            {
                result.Errors.Add(Constants.ValidationErrors.TooFewArguments);
                return result;
            }

            if (args.Length > 0)
            {
                var command = args[0].ToLower();
                if (command != "compress" || command != "decompress")
                {
                    result.Errors.Add(Constants.ValidationErrors.UnknownCommand);
                }
            }

            if (args.Length > 1)
            {
                var fileName = args[1];
                try
                {
                    _ = new FileInfo(fileName);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"{Constants.ValidationErrors.InvalidInputFile} '{fileName}' {ex.Message}");
                }
            }

            if (args.Length > 1)
            {
                var fileName = args[2];
                try
                {
                   _ = new FileInfo(fileName);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"{Constants.ValidationErrors.InvalidOutputFile} '{fileName}' {ex.Message}");
                }
            }

            result.IsValid = result.Errors.Count > 0;
            return result;
        }
    }
}