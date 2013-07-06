/*
    Copyright 2013 Phillip Schichtel

    This file is part of LyricsReloaded.

    LyricsReloaded is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    LyricsReloaded is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with LyricsReloaded. If not, see <http://www.gnu.org/licenses/>.

*/

using System.Text.RegularExpressions;
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

    public class MatchesValidator : Validator
    {
        public string getName()
        {
            return "matches";
        }

        public bool validate(string content, string[] args)
        {
            if (args.Length < 1)
            {
                throw new InvalidConfigurationException("The matches validator needs at least one parameter: matches, <regex>");
            }
            RegexOptions options = RegexOptions.None;
            if (args.Length > 1)
            {
                options = Pattern.optionStringToRegexOptions(args[2].Trim());
            }

            return (new Regex(args[0], options)).Match(content).Success;
        }
    }
}
