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

Docs:

Most of this code is implemented using numeric parsing tables.  These tables have the structure:  

 * row index: state number  
 * col index: character class  
(and the comments on the tables should document these indices)

each element of the table is a pair of numbers, these pairs have the interpretation:

 * first: row index of the next state  
 * second: opCode -- interpreted by Run() in SequentialMachine.cs  

    0: no op  
    1: mark begining of word or sequence  
    2: end word, mark begining of word or sequence  
    3: end word, mark begining of invalid content  
    4: end or continue sequence, mark begining of word or sequence  
    5: end or continue sequence, mark begining of invalid content  

sequences are like words except that we can extend them IF WE DO SO IN THE SAME STATE THAT WE STARTED THEM IN.

the machine processes a sequences as a state machine, one character at a time, with steadily increasing indices into the sequence.

Design credit for this machine: Ken Iverson and Roger Hui (http://www.jsoftware.com/help/dictionary/d332.htm)

Raul Miller  
2012-09-12
