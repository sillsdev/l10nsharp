# ExtractXliff Tool

`ExtractXliff` uses the L10NSharp code for extracting static strings from one or
more C# assemblies (either .dll or .exe).  It requires command line arguments
to set the internal XLIFF file element "original" attribute, to set the
namespace beginning(s), and to set the output XLIFF filename.  It also requires
one or more assembly files to be specified on the command line.  There are
also optional command line arguments for specifying the XLIFF file element
"datatype" attribute, the XLIFF file element "product-version" attribute, and
an existing XLIFF file to merge from after reading everything from the input
assemblies.

## Usage

`ExtractXliff [options] assembly-file(s)`

`-n` or `--namespace`: namespace beginning **[one or more required]**

`-x` or `--xliff-file`: output .xlf file **[one required]**

`-o` or `--original`: file element attribute value **[one required]**

`-d` or `--datatype`: file element attribute value [optional]

`-g` or `--glob`: treat assembly arguments as filename globs instead of files
(directory globs are not supported) [optional]

`-p` or `--product-version`: file element attribute value [optional]

`-b` or `--base-xliff`: existing xliff file to serve as base for output [optional]

`-v` or `--verbose`: produce verbose output on differences from base file [optional]

Every option except `-v` (`--verbose`) consumes a following argument as its value.
The option list can be terminated by "--" in case an assembly filename starts
with a dash ("-").  One or more assembly files (either .dll or .exe) are
required following all of the options.  If a base xliff file is given, then its
content serves as the base for the output, with the extracted strings merged
into, and updating, the existing strings.  Statistics are then written to the
console for the number of new strings, changed strings, identical strings, and
number of strings in the base that were not extracted.
