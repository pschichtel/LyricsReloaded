Filter
======

Filters are small functions that can modify the given content.
Filters can currently be applied to variable values and the lyrics
content.

***Important***: The filters are executed in the specified order, so stripping HTML-tags
                 before converting <br>s to newlines won't get you far.


strip_html
----------
This filter removes all HTML tags from the content.


entity_decode
-------------
This filter decode HTML entity like &copy; -> ©.


strip_links
-----------
This filter removes links from the lyrics.


utf8_encode
-----------
This filter converts the content's encoding to UTF-8 (without BOM).


br2ln
-----
This filter converts <br> tags to newlines (\n).


p2break
-------
This filter converts </p> tags to 2 newlines (\n) indicating a new paragraph.


clean_spaces
------------
This filter cleans up the whitespace of the content by normalizing line endings,
converting tabs to spaces, vertical tabs to newlines and
removing unnecessary newlines and spaces.


trim
----
This filter removes whitespace from the beginning and the end of the content.


lowercase
---------
This filter converts the whole content to lower case.

Note: The convertion is culture unaware.


uppercase
---------
This filter converts the while content to upper case

Note: The convertion is culture unaware.


diacritics2ascii
----------------
This filter removes diacritics from the content, so "äöüß" becomes "aous".


umlauts2ascii
-------------
This filter is specialized version of diacritics2ascii that handles
only the german umlauts and replaces them with their two character
representation, so "äöüß" becomes "aeoeuess".


urlencode
---------
This filter URL-encodes the content where necessary, so a space becomes +.

urlencode
---------
This filter URL-encodes the content where necessary, so a space becomes %20.


regex
-----
This filter does a regex replace, the first argument is the regex (which will be cached)
and the second argument is the replacement which may contain backreferences.
Optionally a third argument can be given which specifies regex options

Example usage: [regex, "\\s+?", " "]


strip_nonascii
--------------
This filter removes all non-ASCII characters.
The filter has 2 optional arguments: The first is a replacement for
the removed character and the second one specifies whether the replacement
can be inserted multiple times in a row.

Examples:

* strip_nonascii                 -> "test *** test" -> "testtest"
* [strip_nonascii, -]            -> "test *** test" -> "test-test"
* [strip_nonascii, -, duplicate] -> "test *** test" -> "test-----test"


replace
-------
This filter replaces the given search string (first argument) with
the replacment (second argument).

Example usage: [replace, search, replace]
