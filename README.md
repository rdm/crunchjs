This program reduces the size of javascript files.
Javascript files should be ascii files.  This program
should handle UTF8 properly, but UTF8 is rather bulky.

This program will not work on files which exceed the
2GB signed integer size limit.

Usage:
	crunchjs input.js output.js


Raul Miller
2007-03-08



Known issues include: it uses newlines as the statement delimiter (for
readability) but sometimes that causes problems.  Also, it will
preserve an extra newline on a comment boundary (this is a minor cost
that does a lot to increase the readability of the minified code).

Raul Miller
2011-11-10
