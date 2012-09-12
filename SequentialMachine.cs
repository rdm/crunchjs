using System;
using System.Collections;

namespace CrunchJS {
	/// <summary>
	/// see http://www.jsoftware.com/help/dictionary/d332.htm
	/// </summary>
	public class SequentialMachine {
		private byte [,,] _states;
		private byte []  _map;
		
		/// <summary>
		/// This will parse byte streams into words
		/// see http://www.jsoftware.com/help/dictionary/d332.htm
		/// </summary>
		/// <param name="states">state control array</param>
		/// <param name="map">character class for each byte</param>
		public SequentialMachine(byte[, ,] states, byte[] map) {
			if (map.Length != 256) throw new InvalidProgramException("expected byte map for 256 distinct bytes");
			if (states.GetLength(2) != 2) throw new InvalidProgramException("invalid state array");
			for (int j= 0; j < map.Length; j++) if (map[j] >= states.GetLength(1)) throw new InvalidProgramException("state array not big enough for all byte classes");
			for (int j= 0; j < states.GetLength(0); j++) 
				for (int k= 0; k < states.GetLength(1); k++) 
					if (states[j,k,0] > states.GetLength(0)) throw new InvalidProgramException("state array refers to a state it does not contain");
					else if (states[j,k,1] > 6) throw new InvalidProgramException("state array contains invalid operation");
			_states= states;
			_map= map;
		}

		/// <summary>
		/// Break the byte stream into a list of individual words
		/// </summary>
		/// <param name="y">byte stream to parse</param>
		/// <returns>resulting words</returns>
		public byte[][] BoxedWords(byte[] y) {
			IList a= Run(y);
			byte[][] result0= new byte[a.Count][];
			for (int i= 0; i < a.Count; i++) {
				int[] cur= (int[])a[i];
				byte[] b= result0[i]= new byte[cur[1]];
				for (int k= 0; k < cur[1]; k++) b[k]= y[cur[0]+k];
			}
			return result0;
		}

		/// <summary>
		/// Parse byte stream, extracting relevant words as a new byte stream
		/// </summary>
		/// <param name="y">byte stream to parse</param>
		/// <returns>relevant byte stream</returns>
		public byte[] Flattened(byte[] y) {
			IList a= Run(y);
			int len= 0;
			for (int i= 0; i < a.Count; i++) len+= ((int[])a[i])[1];
			byte[] result1= new byte[len];
			int start= 0;
			for (int i= 0; i < a.Count; i++) {
				int[] cur= (int[])a[i];
				for (int k= 0; k < cur[1]; k++) result1[start+k]= y[cur[0]+k];
				start+= cur[1];
			}
			return result1;
		}

		/// <summary>
		/// Applies sequential machine to byte stream to determine word boundaries
		/// </summary>
		/// <param name="y">byte stream to parse</param>
		/// <returns>word boundaries</returns>
		private IList Run(byte[] y) {
			ArrayList a= new ArrayList(); // result accumulator
			int jNext= -1; // next begining of word index
			int rOld= -1; // previous state
			int rNext= 0; // next state
			for (int i= 0; i <= y.Length; i++) {
				int r= rNext; // current state
				int j= jNext; // current begining of word
				byte opFn= 0;
				if (i < y.Length) {
					byte c= _map[y[i]]; // current mapped input
					rNext= _states[r, c, 0];
					byte opCode= _states[r, c, 1];
					if (opCode == 6) break;
					switch (opCode) {
						case 0: /*no op*/ break;
						case 1: jNext=  i; break;
						case 2: jNext=  i; opFn= 1; break;
						case 3: jNext= -1; opFn= 1; break;
						case 4: jNext=  i; opFn= 2; break;
						case 5: jNext= -1; opFn= 2; break;
					}
				} else {
					if (j >= 0) opFn= 2;
				}
				switch (opFn) {
					case 0: break;
					case 1: if (0>j) return null; /* emit a new word */
						a.Add(new int[] { j, i-j });
						rOld= -1;
						break;
					case 2: if (0>j) return null; /* possibly extend previous word */
						if (r == rOld) {
							int[] cur= (int[])a[a.Count-1];
							cur[1]= i-cur[0];
						} else {
							a.Add(new int[] { j, i-j });
							rOld= r;
						}
						break;
				}
			}
			return a;
		}
	}
}
