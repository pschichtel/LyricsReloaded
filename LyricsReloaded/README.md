Plugin documentation
====================

Links
-----
* [Filter](Filters/README.md)
* [Validator](Validation/README.md)
* [Example configuration](Configs/example.yml)

Regular expressions
-------------------
This plugin relies heavily on Microsoft-flavored regular expression
for its functionality. Tutorials and tools to write these expressions
are all over the internet, for example [RegExr](http://www.regexr.com/)
which is a greate tool to write and test regular expressions.

Using these regexes is usually done in 2 ways:
1. a single string, which specifies only the regex
2. an array of 2 strings that specifies the regex in its first element
   and regex options as its second element. Example: ["regex", is]

There are 2 things to watch out for:
* backslashes (\\) must be escaped when using double quotes ("), as it is a meta character in the YAML format: \s -> \\\s
* named capturing groups are defined like this: (?\<lyrics\>.*?) with "lyrics" being the name of the group

Regex options
-------------
The regex options are specified as a string that contains the characters
for the options. A lowercase character enables the options, an uppercase
character disables the options.

The options:

* **i**: the regex is case insensitive
* **s**: the input string is seen as a single line
* **m**: the input is seen as multiple lines
* **c**: the regex will be compiled (improves execution performance, but slows startup)
* **x**: whitespace in the regex will be ignored (nice for complex regexes)
* **d**: the regex will go from right to left though the string
* **e**: only named capturing groups will be used
* **j**: the regex will be ECMA script compatible
* **l**: the regex will be culture invariant
