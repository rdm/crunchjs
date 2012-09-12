using System;
using System.IO;
using System.Security.Permissions;
using System.Text;

namespace CrunchJS {
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Crunch {
		static SequentialMachine Cruncher= CrunchMachine();
		static SequentialMachine Cleaner= CleanMachine();
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			if (args.Length != 2) throw new InvalidProgramException("Usage: crunch infile outfile");
			BinaryReader input=new BinaryReader(new FileStream(args[0], FileMode.Open, FileAccess.Read));
			byte[] cleaned= Cleaner.Flattened(input.ReadBytes((int)input.BaseStream.Length));
			byte[] crunched= Cruncher.Flattened(cleaned);
			do {
				crunched= Cleaner.Flattened(cleaned= crunched);
			} while (cleaned.Length != crunched.Length);
			FileStream output= new FileStream(args[1], FileMode.Create, FileAccess.Write);
			output.Write(crunched, 0, crunched.Length);
		}
		static SequentialMachine CleanMachine() {
			byte[] map= new byte[256]; // class 0: keep
			map['{']= 1; // class 1: {
			map[';']= 2; // class 2: ;
			map['}']= 3; // class 3: }
			map['\n']= 4; // class 4: \n
			map['\r']= 5; // class 5: \r
			map[' ']= 6; // class 6: space
			return new SequentialMachine(new byte[,,] {
/*    keep,     {,     ;,     },    \n,    \r,    ' ' */
	{{1,1}, {2,1}, {3,1}, {4,1}, {5,1}, {0,0}, {7,1}}, // discarding previous
	{{1,2}, {2,2}, {3,2}, {4,2}, {5,2}, {0,3}, {7,2}}, // keep
	{{1,2}, {2,2}, {3,2}, {4,2}, {0,3}, {0,3}, {7,2}}, // {
	{{1,2}, {2,2}, {3,2}, {4,1}, {5,1}, {0,3}, {7,2}}, // ;
	{{1,2}, {2,2}, {3,2}, {4,2}, {5,2}, {0,3}, {7,2}}, // }
	{{1,2}, {2,2}, {3,2}, {4,1}, {6,2}, {0,3}, {7,2}}, // LF
	{{1,2}, {2,2}, {3,2}, {4,1}, {0,3}, {0,3}, {7,2}}, // LF,LF
	{{1,2}, {2,2}, {3,2}, {4,2}, {5,1}, {0,3}, {7,2}}  // space
			}, map);
		}
		static SequentialMachine CrunchMachine() {
			byte[] map= new byte[256]; // byte class 0: token forming characters (significant, but no special rules)
			map['\n']= 1; // byte class 1: newline character
			map[9]=map[11]=map[12]=map[13]=map[32]=2; // byte class 2: whitespace
			map['\'']= 3; // byte class 3: single quote
			map['"']= 4; // byte class 4: double quote
			map['\\']= 5; // byte class 5: escape character
			map['/']= 6; // byte class 6: begining of comment or regexp
			map['*']= 7; // byte class 7: multi-line comment indicator
			map['[']= 8; // byte class 8: begining of regexp character class
			map[']']= 9; // byte class 9: end of regexp character class
			byte[] wordForming= ASCIIEncoding.ASCII.GetBytes("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_$");
			for (int j= 0; j < wordForming.Length; j++) map[wordForming[j]]= 10; // byte class 10: word forming characters
			for (int j= 128; j < 256; j++) map[j]= 10; // treat non-ascii characters as potentially word-forming
			return new SequentialMachine(new byte[,,] {
/* token,     \n,       ,      ',      ",      \,      /,      *,      [,      ],   abcde, */
  {{1,1},  {0,0},  {0,0},  {2,1},  {4,1},  {1,1},  {6,1},  {1,1},  {1,1},  {1,1}, {14,1}}, //- irrelevant whitespace
  {{1,2}, {17,2}, {18,3},  {2,2},  {4,2},  {1,2},  {6,2},  {1,2},  {1,2},  {1,2}, {14,2}}, //  +-= etc.
  {{2,2}, {17,2},  {2,2},  {1,2},  {2,2},  {3,2},  {2,2},  {2,2},  {2,2},  {2,2},  {2,2}}, //  '
  {{2,2}, {17,2},  {2,2},  {2,2},  {2,2},  {2,2},  {2,2},  {2,2},  {2,2},  {2,2},  {2,2}}, //  '\
  {{4,2}, {17,2},  {4,2},  {4,2},  {1,2},  {5,2},  {4,2},  {4,2},  {4,2},  {4,2},  {4,2}}, //  "
  {{4,2}, {17,2},  {4,2},  {4,2},  {4,2},  {4,2},  {4,2},  {4,2},  {4,2},  {4,2},  {4,2}}, //  "\
 {{10,2}, {17,2}, {10,2}, {10,2}, {10,2}, {10,2},  {7,0},  {8,0}, {11,2}, {10,2}, {10,2}}, //  /
  {{7,0}, {17,1},  {7,0},  {7,0},  {7,0},  {7,0},  {7,0},  {7,0},  {7,0},  {7,0},  {7,0}}, //  //
  {{8,0},  {8,0},  {8,0},  {8,0},  {8,0},  {8,0},  {8,0},  {9,0},  {8,0},  {8,0},  {8,0}}, //  /*
  {{8,0},  {8,0},  {8,0},  {8,0},  {8,0},  {8,0},  {0,0},  {9,0},  {8,0},  {8,0},  {8,0}}, //  /*...*
 {{10,2}, {17,2}, {10,2}, {10,2}, {10,2}, {12,2},  {1,2}, {10,2}, {11,2}, {10,2}, {10,2}}, //  /.
 {{11,2}, {17,2}, {11,2}, {11,2}, {11,2}, {13,2}, {11,2}, {11,2}, {11,2}, {10,2}, {11,2}}, //  /[
 {{10,2}, {10,2}, {10,2}, {10,2}, {10,2}, {10,2}, {10,2}, {10,2}, {10,2}, {10,2}, {10,2}}, //  /\
 {{11,2}, {11,2}, {11,2}, {11,2}, {11,2}, {11,2}, {11,2}, {11,2}, {11,2}, {11,2}, {11,2}}, //  /[\
  {{1,2}, {17,2}, {15,2},  {2,2},  {4,2},  {1,2},  {6,2},  {1,2},  {1,2},  {1,2}, {14,2}}, //  word
  {{1,1}, {17,1}, {15,1},  {2,1},  {4,1},  {1,1},  {6,2},  {1,1},  {1,1},  {1,1}, {14,2}}, //  space right after word
  {{1,1}, {17,1}, {16,0},  {2,1},  {4,1},  {1,2},  {6,1},  {1,1},  {1,1},  {1,1}, {14,1}}, //- further space before LF
  {{1,2},  {0,2},  {0,2},  {2,2},  {4,2},  {1,2},  {6,2},  {1,2},  {1,2},  {1,2}, {14,2}}, //  first LF
  {{1,1}, {17,1}, {18,0},  {2,1},  {4,1},  {1,1},  {6,1},  {1,1},  {1,1},  {1,1}, {14,1}}  //- space after token
													  }, map);
		}
	}
}
