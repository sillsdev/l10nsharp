# CheckOrFixXliff Tool

This program optionally validates XLiff files in three ways:

1) Check for being well-formed XML.  This is the most basic check, and must pass for anything else
   to work.

2) Check against the XLiff 1.2 schema.  This is optional (`--validate` flag), and not really needed
   for Bloom and other L10NSharp clients who ignore the exact order of the child elements in
   trans-unit elements.  Note that crowdin doesn't always produce valid Xliff, and they don't seem
   all that concerned about fixing it.

3) Check all translated format strings (those that contain markers like `{0}` for validity: markers
   match between the source and target strings, and the markers in the target strings are not
   mangled in translation.

   If malformed markers are detected that would cause the program to crash, the 3 characters
   preceding and 6 characters following each open brace (`'{'`) are displayed following the warning
   messages for the string.

If the third check reveals problems involving mangled substitution markers (most often in RTL
scripts), the program can optionally try to repair the strings using some common patterns that have
been observed (`--fix` flag).
