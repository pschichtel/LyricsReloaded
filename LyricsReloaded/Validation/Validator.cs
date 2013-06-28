using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CubeIsland.LyricsReloaded.Provider;

namespace CubeIsland.LyricsReloaded.Validation
{
    public interface Validator
    {
        string getName();
        bool validate(string content, string[] args);
    }

    public class Validation
    {
        private readonly Validator validator;
        private readonly bool inverted;
        private readonly string[] args;

        public Validation(Validator validator, bool inverted, string[] args)
        {
            this.validator = validator;
            this.inverted = inverted;
            this.args = args;
        }

        public bool execute(string content)
        {
            bool result = this.validator.validate(content, this.args);
            if (this.inverted)
            {
                result = !result;
            }
            return result;
        }
    }

    public class ContainsValidator : Validator
    {
        public string getName()
        {
            return "contains";
        }

        public bool validate(string content, string[] args)
        {
            if (args.Length < 1)
            {
                throw new InvalidConfigurationException("The contains validator needs at least 1 argument: contains, <text>[, <ci>]");
            }
            if (args.Length > 1)
            {
                return content.ToLower().Contains(args[0].ToLower());
            }
            return content.Contains(args[0]);
        }
    }
}
