namespace GZipTest.CommandLineArguments
{
    public interface ICommandLineValidator
    {
        ValidationResult Validate(string[] args);
    }
}