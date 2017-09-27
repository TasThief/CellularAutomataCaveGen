///-----------------------------------------------------------------
///   Struct:         SwapChain
///   Description:    SwapChain Design patern: 
///                   It contains 2 buffers holding the information of the map being generated.
///                   These buffers have a swapping mechanism to auxiliate the process of the cellular automata.
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace CaveMapGenerator {
	/// <summary>
	/// This struct holds a simple structure of buffers, 
	/// these buffers are the canvas for the celular automata
	/// </summary>
	public struct Swapchain : ICloneable, IEnumerable<bool> {
		//-----------------------------------DATA------------------------------------
		/// <summary>
		/// Internal buffers
		/// </summary>
		private bool[,]
			writeBuffer,
			readBuffer;

		//-------------------------DATA ACCESS PROPERTIES--------------------------
		/// <summary>
		/// Buffer indexer
		/// </summary>
		/// <param name="x">Coordinate x</param>
		/// <param name="y">Coorinate y</param>
		/// <param name="value">data written on the write buffer</param>
		/// <returns>data on the read buffer</returns>
		public bool this[int x, int y] {
			get { return readBuffer[x, y]; }
			set { writeBuffer[x, y] = value; }
		}

		/// <summary>
		/// Buffer indexer
		/// </summary>
		/// <param name="c">coordinate</param>
		/// <param name="value">data written on the write buffer</param>
		/// <returns>data on the read buffer</returns>
		public bool this[Coordinate c] {
			get { return readBuffer[c.x, c.y]; }
			set { writeBuffer[c.x, c.y] = value; }
		}
		
		/// <param name="c">given coodinate</param>
		/// <returns> if coordinate is outside buffer's bounds</returns>
		public bool IsCoordinateOutsideBounds( Coordinate c ) {
			return  ( c.x < 0 || c.y < 0 || c.x >= Size || c.y >= Size ) ;
		}
		
		/// <param name="c">given coodinate</param>
		/// <returns> if coordinate is within buffer's bounds</returns>
		public bool IsCoordinateWithinBounds( Coordinate c ) {
			return !IsCoordinateOutsideBounds(c);
		}

		/// <summary>
		/// Buffer bounds
		/// </summary>
		public int Size {
			get { return readBuffer.GetLength(0); }
		}

		//---------------------------------FUNCTIONS---------------------------------
		/// <summary>
		/// return the count of alive cells around a given coordinate
		/// </summary>
		/// <param name="c">coordinate</param>
		/// <param name="range">count range</param>
		/// <returns></returns>
		public int CountAliveAdjacentCells( Coordinate c , int range) {
			int result = 0;
			Coordinate auxCoord = new Coordinate();
			//run on a range X range matrix
			for ( int j = -range; j <= range; j++ )
				for ( int i = -range; i <= range; i++ ) {

					//in-buffer coordinate
					auxCoord.x = i + c.x;
					auxCoord.y = j + c.y;

					//If its not middle cell
					if ( !( i == 0 && j == 0 ) &&

					   //If coordinates are outside bounds   (outside bounds cells are count as filled)
					   ( IsCoordinateOutsideBounds(auxCoord) ||

					     //if the cell is "true"
					     this[auxCoord] ) )

						//add 1 to the counter
						result++;
				}
			//return counter
			return result;
		}

		/// <summary>
		/// Swap read and write buffers
		/// </summary>
		public void Swap() {
			bool[,] auxiliarBuffer = writeBuffer;
			writeBuffer = readBuffer;
			readBuffer = auxiliarBuffer;
		}

		/// <summary>
		/// Get a copy of the read buffer to external operations
		/// </summary>
		public bool[,] GetBufferCopy() {
			return readBuffer.Clone() as bool[,];
		}

		/// <summary>
		/// Deep clone this swapchain
		/// </summary>
		/// <returns>a copy of this swapchain</returns>
		public object Clone() {
			return new Swapchain(this);
		}

		//------------------------------READ AND WRITE-------------------------------
		/// <summary>
		/// Read the buffer
		/// </summary>
		public IEnumerator<bool> GetEnumerator() {
			foreach ( bool cell in this )
				yield return cell;
		}

		/// <summary>
		/// Read the buffer
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator() {
			for ( int y = 0; y < Size; y++ )
				for ( int x = 0; x < Size; x++ )
					yield return readBuffer[x, y];
		}
		
		/// <summary>
		/// Read the data inside the buffers
		/// </summary>
		/// <param name="function">Read iteration function, Coordinate = cell coordinate </param>
		public void Read(Action<Coordinate, bool> function ) {
			Coordinate c = new Coordinate();
			for ( c.y = 0; c.y < Size; c.y++ )
				for ( c.x = 0; c.x < Size; c.x++ )
					function(c, this[c]);
		}
		
		/// <summary>
		/// Iterate the buffers and write data into the writeBuffer
		/// </summary>
		/// <param name="function">Iteration function, Coordinate = iterated coordinate, bool = cell value, return = saved data into the write buffer></param>
		public void Write( Func<Coordinate, bool, bool> function ) {
			bool[,] writeref = writeBuffer;
			Read(( c, cell ) => writeref[c.x, c.y] = function(c, cell));
			Swap();
		}   
		
		/// <summary>
		/// Iterate the buffers and write data into the writeBuffer
		/// </summary>
		/// <param name="function">Iteration function, bool = cell value, return = saved data into the write buffer></param>
		public void Write( Func<bool, bool> function ) {
			Write(( c, cell ) => function(cell));
		}
		
		/// <summary>
		/// Iterate the buffers and write data into the writeBuffer
		/// </summary>
		/// <param name="function">Iteration function, Coordinate = iterated coordinate, return = saved data into the write buffer></param>
		public void Write( Func<Coordinate, bool> function ) {
			Write(( c, cell ) => function(c));
		}
		
		/// <summary>
		/// Iterate the buffers and write data into the writeBuffer
		/// </summary>
		/// <param name="function">Iteration function, return = saved data into the write buffer></param>
		public void Write( Func<bool> function ) {
			Write(( c, cell ) => function());
		}
		
		/// <summary>
		/// Copy external buffer into the data structure. Buffers need to have the same size
		/// </summary>
		/// <param name="buffer">the buffer to be copied</param>
		public void Write(bool[,] buffer ) {
			if ( buffer.GetLength(0) == Size ) {
				writeBuffer = buffer.Clone() as bool[,];
				Swap();
			} else
				throw new Exception("Atempt to write an unmatching buffer into the swapchain");
		}
		
		/// <summary>
		/// Write data using a coordinate list as mask
		/// </summary>
		/// <param name="mask">mask as coordinate list</param>
		/// <param name="baseFunction">what should be written on the buffer</param>
		/// <param name="maskFunction">what should be written on the masked part of the buffer</param>
		public void Write( List<Coordinate> mask, Func<bool> baseFunction, Func<bool> maskFunction ) {
			bool[,] writeref = writeBuffer;
			Read(( c, cell ) => writeref[c.x, c.y] = baseFunction());
			foreach ( Coordinate coordinate in mask )
				this[coordinate] = maskFunction();
			Swap();
		}
		
		/// <summary>
		/// Copy read buffer into the write buffer
		/// </summary>
		public void CopyReadToWrite() {
			writeBuffer = readBuffer.Clone() as bool[,];
		}

		//-------------------------------CONSTRUCTORS-------------------------------
		/// <summary>
		/// Build a new buffer set with the given size
		/// </summary>
		/// <param name="size">square size of the buffer</param>
		public Swapchain( int size ) {
			writeBuffer = new bool[size, size];
			readBuffer = new bool[size, size];
		}
		
		/// <summary>
		/// Build a new buffer set with the given size
		/// </summary>
		/// <param name="size">square size of the buffer</param>
		public static implicit operator Swapchain(int size ) {
			return new Swapchain(size);
		}

		/// <summary>
		/// Build a new Swapchain using a buffer as base
		/// </summary>
		/// <param name="buffer">the base buffer</param>
		public Swapchain( bool[,] buffer ) {
			writeBuffer = buffer.Clone() as bool[,];
			readBuffer = buffer.Clone() as bool[,];
		}
		
		/// <summary>
		/// Build a new Swapchain using a buffer as base
		/// </summary>
		/// <param name="buffer">the base buffer</param>
		public static implicit operator Swapchain(bool[,] buffer ) {
			return new Swapchain(buffer);
		}

		/// <summary>
		/// Build a swapchain from another swapchain (clone)
		/// </summary>
		/// <param name="previousSwapChain">the base swapchain for clonnage</param>
		public Swapchain( Swapchain previousSwapChain ) {
			writeBuffer = previousSwapChain.writeBuffer.Clone() as bool[,];
			readBuffer = previousSwapChain.readBuffer.Clone() as bool[,];
		}
		
		/// <summary>
		/// Build a swapchain from another swapchain (clone)
		/// </summary>
		/// <param name="previousSwapChain">the base swapchain for clonnage</param>
		public static implicit operator int(Swapchain obj ) {
			return obj.Size;
		}
	}
	public struct Coordinate {
		public int x, y;
		public Coordinate( int x = 0, int y = 0 ) {
			this.x = x;
			this.y = y;
		}
	}

	public class Hole {
		public int count;
		public List<Coordinate> holeCells;

		public Hole() {
			holeCells = new List<Coordinate>();
			count = 0;
		}
		public void AddCell( int x, int y ) {
			holeCells.Add(new Coordinate(x, y));
			count++;
		}
	}

}