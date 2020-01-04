using System.Linq;
using GZipTest.CommandLineArguments;
using Shouldly;
using Xunit;

namespace GZipTest.Tests.CommandLineArguments
{
    public class CommandLineValidatorTests
    {
        private readonly ICommandLineValidator validator;

        public CommandLineValidatorTests()
        {
            validator = new CommandLineValidator();
        }

        [Fact]
        public void EmptyArgsReturnOneError()
        {
            var validationResult = validator.Validate(new string[0]);
            validationResult.ShouldSatisfyAllConditions(
                () => validationResult.IsValid.ShouldBeFalse(),
                () => validationResult.Errors.Count.ShouldBe(1),
                () => validationResult.Errors.First().ShouldBe(Constants.ValidationErrors.TooFewArguments)
                );
        }
    }
}
