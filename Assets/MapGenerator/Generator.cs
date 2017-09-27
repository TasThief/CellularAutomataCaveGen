///-----------------------------------------------------------------
///   Class:          Generator
///   Description:    It uses cellular automata to generate a cave complex
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace CaveMapGenerator {
	public class Generator {
		/// <summary>
		/// the swapchain containing the buffers that will receive the operations
		/// </summary>
		private Swapchain map = new Swapchain(100);

		public bool[,] GetBufferCopy() {
			return map.GetBufferCopy();
		}

		public int Size { get { return map.Size; } }

		/// <summary>
		/// Fill a buffer with random noise
		/// </summary>
		public void NewMap( int size, float initialDensity ) {
			//reset the swapchain if needed
			if ( size != map )
				map = size;
			map.Write(() => Random.value < initialDensity);
		}

		/// <summary>
		/// Iterate through the cells to refine the cave
		/// </summary>
		public void Refine( float minThreshold, float maxThreshold ) {
			int count;
			map.Write(( coordinate, cell ) => {
				count = map.CountAliveAdjacentCells(coordinate,1);
				return ( cell ) ?
					count >= minThreshold :
					count > maxThreshold;
			});
		}
		public void RemoveIsle(Coordinate c ) {
			bool[,] buffer = map.GetBufferCopy();
			Flood(ref buffer, c.x, c.y, !buffer[c.x, c.y]);
			map.Write(buffer);
		}
		/// <summary>
		/// Recursive method that floods the cave to discover holes in it.
		/// </summary>
		public void Flood( ref bool[,] buffer, int x, int y , bool target) {
			//if this cell is valid and it's empty
			if ( !( x < 0 || y < 0 || x >= buffer.GetLength(0) || y >= buffer.GetLength(0) ) &&  buffer[x,y] != target ) {

				//write fill it
				buffer[x, y] = target;

				//reverberate to adjacent cells
				Flood(ref buffer, x + 1, y, target);
				Flood(ref buffer, x - 1, y, target);
				Flood(ref buffer, x, y + 1, target);
				Flood(ref buffer, x, y - 1, target);
			}
		}

		/// <summary>
		/// Recursive method that floods the cave to discover holes in it, it fills a hole object that was given during the method invocation
		/// </summary>
		/// <param name="hole">The hole that is going to be filled</param>
		private void Flood( ref bool[,] buffer, Hole hole, int x, int y ) {
			//if this cell is valid and it's empty
			if ( !( x < 0 || y < 0 || x >= buffer.GetLength(0) || y >= buffer.GetLength(0) ) &&
			    !buffer[x, y] ) {
				//write fill it
				buffer[x, y] = true;

				//add its coordinate to the hole's list
				hole.AddCell(x, y);

				//reverberate to adjacent cells
				Flood(ref buffer, hole, x + 1, y);
				Flood(ref buffer, hole, x - 1, y);
				Flood(ref buffer, hole, x, y + 1);
				Flood(ref buffer, hole, x, y - 1);
			}
		}

		/// <summary>
		/// Read the buffers and identify 'island' holes 
		/// </summary>
		public int AutoIdentifyHoles() {
			//Initialize a hole list to hold every hole this map has
			List<Hole> holes = new List<Hole>();

			//this reference has multiple purposes, it is generaly used as an auxiliar reference.
			Hole auxHoleRef = null;

			//extract buffer from data structure for more complex operations.
			bool[,] buffer = map.GetBufferCopy();

			//extract isles from buffer
			Tools.Foreach2D(buffer, (int x,int y, ref bool cell) => {
				if ( !cell ) {
					//create a hole
					auxHoleRef = new Hole();

					//run a recursive method to find every other open cell connected to this one
					//this method also fills the hole that was given
					//and closes whatever cell is conected to this one (to avoid to copy the information twice)
					Flood(ref buffer, auxHoleRef, x, y);

					//Add this hole to the list of holes
					holes.Add(auxHoleRef);
				}
			});

			//clean the auxiliar variable
			auxHoleRef = null;

			//get the biggest hole found (the main cavern)
			foreach ( var hole in holes )
				if ( auxHoleRef == null || auxHoleRef.count < hole.count )
					auxHoleRef = hole;

			//if there's a hole (it might be full black)
			if ( auxHoleRef != null )
				map.Write(auxHoleRef.holeCells, () => true, () => false);

			return ( auxHoleRef != null ) ? auxHoleRef.count : 0;
		}
		/// <summary>
		/// Fully generate a map using the stored map generation data
		/// </summary>
		public void GenerateMap( int refinementSteps, float minThreshold, float maxThreshold, float initialDensity, int mapSize ) {
			//the actual size of the newly generated map
			int size = 0;

			//a nice debug message that is displayed at the console
			string debugMessage = "";

			//initialize the map
			NewMap(mapSize, initialDensity);

			//refine it some times
			for ( int i = 0; i < refinementSteps; i++ )
				Refine(minThreshold, maxThreshold);

			//auto identify holes
			size = AutoIdentifyHoles();

			//write the debug log
			debugMessage += "Map generated with size " + size + " cells\n";
		}

		/// <summary>
		/// DEPRECATED, use the other overload
		/// Fully generate a map using the stored map generation data
		/// </summary>
		public void GenerateMap( int minCaveSize, int maxCaveSize, int refinementSteps, float minThreshold, float maxThreshold, float initialDensity, int mapSize ) {
			//the actual size of the newly generated map
			int size = 0;

			//this variable holds the how much trials were done before quiting to get a map that has the right size
			int safeLock = 0;

			//a nice debug message that is displayed at the console
			string debugMessage = "";

			//for as long as we dont have a map what fits the minmun required size
			do {
				//add one attempt to the safe lock
				safeLock++;

				//initialize the map
				NewMap(mapSize, initialDensity);

				//refine it some times
				for ( int i = 0; i < refinementSteps; i++ )
					Refine(minThreshold, maxThreshold);

				//auto identify holes
				size = AutoIdentifyHoles();

				//write the debug log
				debugMessage += "Atempt number " + safeLock + ", Map size = " + size + " cells\n";

				//check is the generated map is fit
			} while ( ( size < minCaveSize || size > maxCaveSize ) && safeLock < 10 );

			//if the map is not ideal
			if ( safeLock >= 10 )
				//send an warning
				Debug.LogWarning("Map generated is not ideal\n" + debugMessage);
			else
				//if its ideal send a ok message
				Debug.Log("Map with accepted size generated\n" + debugMessage);

		}

	}
}