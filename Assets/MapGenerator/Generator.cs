///-----------------------------------------------------------------
///   Class:          Generator
///   Description:    It uses cellular automata to generate a cave complex
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;


//-----------------------------------------------------------


/*
namespace CaveMapGenerator
{
	public class Generator
	{
		/// <summary>
		/// the swapchain containing the buffers that will receive the operations
		/// </summary>
		private SwapChain swapChain = new SwapChain(100);

		/// <summary>
		/// Provides the top buffer of the swapchain
		/// </summary>
		public bool[,] Map { get { return swapChain.ReadBuffer; } }

		/// <summary>
		/// size of the map generated
		/// </summary>
		public int Size { get { return Map.GetLength(0); } }
		
		/// <summary>
		/// Counts how much cells around the given coodinate are alive
		/// </summary>
		/// <returns>the ammount of true cells</returns>
		private int CountAliveAdjacentCells(int cellX, int cellY)
		{
			int result = 0, x, y;

			for(int j = -1; j < 2; j++)
				for(int i = -1; i < 2; i++) {
					x = i + cellX;
					y = j + cellY;

					if(!( i == 0 && j == 0 ) &&
					   ( ( x < 0 || y < 0 || x >= Size || y >= Size ) ||
						 swapChain.WriteBuffer[x, y] ))
						result++;
				}
			return result;
		}

		/// <summary>
		/// Iterate through the cells to refine the cave
		/// </summary>
		public void Refine(float minThreshold, float maxThreshold)
		{
			swapChain.FlipBuffers();

			int count;

			Tools.Foreach2D(swapChain.WriteBuffer, (int x, int y, ref bool cell) => {
				count = CountAliveAdjacentCells(x, y);
				Map[x, y] = ( cell ) ?
					 count >= minThreshold :
					 count > maxThreshold;
			});
		}

		/// <summary>
		/// Recursive method that floods the cave to discover holes in it.
		/// </summary>
		public void Flood(int x, int y)
		{
			//if this cell is valid and it's empty
			if(!( x < 0 || y < 0 || x >= Size || y >= Size ) &&
				!swapChain.ReadBuffer[x, y]) {
				//write fill it
				swapChain.ReadBuffer[x, y] = true;

				//reverberate to adjacent cells
				Flood(x + 1, y);
				Flood(x - 1, y);
				Flood(x, y + 1);
				Flood(x, y - 1);
			}
		}

		/// <summary>
		/// Recursive method that floods the cave to discover holes in it, it fills a hole object that was given during the method invocation
		/// </summary>
		/// <param name="hole">The hole that is going to be filled</param>
		private void Flood(Hole hole, int x, int y)
		{
			//if this cell is valid and it's empty
			if(!( x < 0 || y < 0 || x >= Size || y >= Size ) &&
				!swapChain.WriteBuffer[x, y]) {
				//write fill it
				swapChain.WriteBuffer[x, y] = true;

				//add its coordinate to the hole's list
				hole.AddCell(x, y);

				//reverberate to adjacent cells
				Flood(hole, x + 1, y);
				Flood(hole, x - 1, y);
				Flood(hole, x, y + 1);
				Flood(hole, x, y - 1);
			}
		}

		/// <summary>
		/// Read the buffers and identify 'island' holes 
		/// </summary>
		public int AutoIdentifyHoles()
		{
			//Flip the buffers
			swapChain.FlipBuffers();

			//Initialize a hole list to hold every hole this map has
			List<Hole> holes = new List<Hole>();

			//this reference has multiple purposes,
			//it is generaly used as an auxiliar reference.
			Hole auxiliarHoleReference = null;

			//for each cell in the buffer
			Tools.Foreach2D(swapChain.WriteBuffer, (int x, int y, ref bool cell) => {

				//if that cell is is open
				if(!cell) {
					//create a hole
					auxiliarHoleReference = new Hole();

					//run a recursive method to find every other open cell connected to this one
					//this method also fills the hole that was given
					//and closes whatever cell is conected to this one (to avoid to copy the information twice)
					Flood(auxiliarHoleReference, x, y);

					//Add this hole to the list of holes
					holes.Add(auxiliarHoleReference);
				}
			});

			//Flip the buffers
			swapChain.FlipBuffers();

			//clean the auxiliar variable
			auxiliarHoleReference = null;

			//get the bigger hole found (the main cavern
			foreach(var hole in holes)
				if(auxiliarHoleReference == null || auxiliarHoleReference.count < hole.count)
					auxiliarHoleReference = hole;

			//if there's a hole in the entire map (it might be full black
			if(auxiliarHoleReference != null) {
				//set the entire buffer to be black
				Tools.Foreach2D(swapChain.WriteBuffer, (ref bool cell) => cell = true);

				//for each cell recorded at the hole object, paint the buffer white
				foreach(Coordinate coordinate in auxiliarHoleReference.holeCells)
					swapChain.WriteBuffer[coordinate.x, coordinate.y] = false;
			}

			//Flip the buffers
			swapChain.FlipBuffers();

			return ( auxiliarHoleReference != null ) ? auxiliarHoleReference.count : 0;
		}

		/// <summary>
		/// Fully generate a map using the stored map generation data
		/// </summary>
		public void GenerateMap(int refinementSteps, float minThreshold, float maxThreshold, float initialDensity, int mapSize)
		{
			//the actual size of the newly generated map
			int size = 0;

			//a nice debug message that is displayed at the console
			string debugMessage = "";

			//initialize the map
			NewMap(mapSize, initialDensity);

			//refine it some times
			for(int i = 0; i < refinementSteps; i++)
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
		public void GenerateMap(int minCaveSize, int maxCaveSize, int refinementSteps, float minThreshold, float maxThreshold, float initialDensity, int mapSize)
		{
			//the actual size of the newly generated map
			int size = 0;

			//this variable holds the how much trials were done before quiting to get a map that has the right size
			int safeLock = 0;

			//a nice debug message that is displayed at the console
			string debugMessage = "";

			//for as long as we dont have a map what fits the minmun required size
			do {
				//add one trial to the safe lock
				safeLock++;

				//initialize the map
				NewMap(mapSize, initialDensity);

				//refine it some times
				for(int i = 0; i < refinementSteps; i++)
					Refine(minThreshold, maxThreshold);

				//auto identify holes
				size = AutoIdentifyHoles();

				//write the debug log
				debugMessage += "Atempt number " + safeLock + ", Map size = " + size + " cells\n";

				//check is the generated map is fit
			} while(( size < minCaveSize || size > maxCaveSize ) && safeLock < 10);

			//if the map is not ideal
			if(safeLock >= 10)
				//send an warning
				Debug.LogWarning("Map generated is not ideal\n" + debugMessage);
			else
				//if its ideal send a ok message
				Debug.Log("Map with accepted size generated\n" + debugMessage);

		}

	}
}


//-----------------------------------------------------------------------------


namespace CaveMapGenerator
{
	public class Generator
	{
		/// <summary>
		/// the swapchain containing the buffers that will receive the operations
		/// </summary>
		private Swapchain map = new Swapchain(100);

		public bool[,] GetBufferCopy()
		{
			return map.GetBufferCopy();
		}

		public int Size { get { return map.Size; } }

		/// <summary>
		/// Fill a buffer with random noise
		/// </summary>
		public void NewMap(int size, float initialDensity)
		{
			//reset the swapchain if needed
			if(size != map)
				map = size;
			map.Write(() => Random.value < initialDensity);
		}

		/// <summary>
		/// Iterate through the cells to refine the cave
		/// </summary>
		public void Refine(float minThreshold, float maxThreshold)
		{
			int count;
			map.Write((coordinate, cell) => {
				count = map.CountAliveAdjacentCells(coordinate, 1);
				return ( cell ) ?
					count >= minThreshold :
					count > maxThreshold;
			});
		}

		public void RemoveIsle(Coordinate c)
		{
			bool[,] buffer = map.GetBufferCopy();
			Flood(ref buffer, c.x, c.y, !buffer[c.x, c.y]);
			map.Write(buffer);
		}
		/// <summary>
		/// Recursive method that floods the cave to discover holes in it.
		/// </summary>
		public void Flood(ref bool[,] buffer, int x, int y, bool target)
		{
			//if this cell is valid and it's empty
			if(!( x < 0 || y < 0 || x >= buffer.GetLength(0) || y >= buffer.GetLength(0) ) && buffer[x, y] != target) {

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
		private void Flood(ref bool[,] buffer, Hole hole, int x, int y)
		{
			//if this cell is valid and it's empty
			if(!( x < 0 || y < 0 || x >= buffer.GetLength(0) || y >= buffer.GetLength(0) ) &&
				!buffer[x, y]) {
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
		public int AutoIdentifyHoles()
		{
			//Initialize a hole list to hold every hole this map has
			List<Hole> holes = new List<Hole>();

			//this reference has multiple purposes, it is generaly used as an auxiliar reference.
			Hole auxHoleRef = null;

			//extract buffer from data structure for more complex operations.
			bool[,] buffer = map.GetBufferCopy();

			//extract isles from buffer
			Tools.Foreach2D(buffer, (int x, int y, ref bool cell) => {
				if(!cell) {
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
			foreach(var hole in holes)
				if(auxHoleRef == null || auxHoleRef.count < hole.count)
					auxHoleRef = hole;

			//if there's a hole (it might be full black)
			if(auxHoleRef != null)
				map.Write(auxHoleRef.holeCells, () => true, () => false);

			return ( auxHoleRef != null ) ? auxHoleRef.count : 0;
		}
		
		/// <summary>
		/// Fully generate a map using the stored map generation data
		/// </summary>
		public void GenerateMap(int refinementSteps, float minThreshold, float maxThreshold, float initialDensity, int mapSize)
		{
			//the actual size of the newly generated map
			int size = 0;

			//a nice debug message that is displayed at the console
			string debugMessage = "";

			//initialize the map
			NewMap(mapSize, initialDensity);

			//refine it some times
			for(int i = 0; i < refinementSteps; i++)
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
		public void GenerateMap(int minCaveSize, int maxCaveSize, int refinementSteps, float minThreshold, float maxThreshold, float initialDensity, int mapSize)
		{
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
				for(int i = 0; i < refinementSteps; i++)
					Refine(minThreshold, maxThreshold);

				//auto identify holes
				size = AutoIdentifyHoles();

				//write the debug log
				debugMessage += "Atempt number " + safeLock + ", Map size = " + size + " cells\n";

				//check is the generated map is fit
			} while(( size < minCaveSize || size > maxCaveSize ) && safeLock < 10);

			//if the map is not ideal
			if(safeLock >= 10)
				//send an warning
				Debug.LogWarning("Map generated is not ideal\n" + debugMessage);
			else
				//if its ideal send a ok message
				Debug.Log("Map with accepted size generated\n" + debugMessage);

		}

	}
}

	*/








//------------------------------------------------------------


namespace CaveMapGenerator
{
	public class Generator
	{
		public class Bubble
		{
			public int count;
			public List<Coordinate> cells;

			public Bubble()
			{
				cells = new List<Coordinate>();
				count = 0;
			}
			public void AddCell(int x, int y)
			{
				cells.Add(new Coordinate(x,y));
				count++;
			}
		}

		/// <summary>
		/// the swapchain containing the buffers that will receive the operations
		/// </summary>
		private Swapchain<bool> map = new Swapchain<bool>(128);
		private Swapchain<float> height = new Swapchain<float>(128);

		/// <summary>
		/// Return a copy of the read buffer stored inside the map
		/// </summary>
		public bool[,] GetMapBufferCopy()
		{
			return map.GetBufferCopy();
		}
	
		/// <summary>
		/// Return a copy of the read buffer stored inside the map
		/// </summary>
		/// 
		public float[,] GetHeightBufferCopy()
		{
			return height.GetBufferCopy();
		}

		/// <summary>
		/// Return the size of the map
		/// </summary>
		public int Size { get { return map.Size; } }

		/// <summary>
		/// Fill a buffer with random noise
		/// </summary>
		public void NewMap(int size, float initialDensity, AnimationCurve spreadCurve)
		{
			//reset the swapchain if needed
			if(size != map) {
				map = size;
				height = size;
			}
			map.Write((Coordinate c) => {
				float x = ( ( (float)c.x / (float)size ) - 0.5f ) * 1.5f;
				float y = ( ( (float)c.y / (float)size ) - 0.5f ) * 1.5f;
				return Random.value < spreadCurve.Evaluate(Mathf.Sqrt(( x * x ) + ( y * y )));
			});

			float rndPositionX = Random.Range(-50, 50);
			float rndPositionY = Random.Range(-50, 50);
			float rndDimention = 0.05f;
			height.Write((Coordinate c) => {
				return Mathf.PerlinNoise(
					( (float)c.x + rndPositionX ) * rndDimention,
					( (float)c.y + rndPositionY ) * rndDimention);
			});
		}

		/// <summary>
		/// Iterate through the cells to refine the cave
		/// </summary>
		public void Refine(float minThreshold, float maxThreshold)
		{
			int count;
			bool result;
			map.Write((coordinate, cell) => {
				count = CountAliveAdjacentCells(coordinate, 1);
				result = ( cell ) ? count >= minThreshold : count > maxThreshold;
				return result;
			});
			height.Write((coordinate, cell) => {
				return GetMass(1, coordinate);
			});
		}

		/// <summary>
		/// Find isles and remove them
		/// </summary>
		/// <param name="c">anchor cell for the bubble removal algorithm</param>
		public void RemoveIsle(Coordinate c)
		{
			bool[,] buffer = map.GetBufferCopy();
			Flood(ref buffer, c.x,c.y, !buffer[c.x, c.y]);
			map.Write(buffer);
		}

		/// <summary>
		/// Remove noise from the main chamber
		/// </summary>
		/// <param name="maxPilarSize">max noise size (to be considered  a noise)</param>
		public void RemovePillars(int maxPilarSize)
		{
			List<Coordinate> mask = new List<Coordinate>();
			List<Bubble> bubblelist = ListBubbles(true);
			foreach(Bubble pilar in bubblelist)
				if(pilar.count <= maxPilarSize)
					mask.AddRange(pilar.cells);
			map.Write(mask, () => false);
		}

		/// <summary>
		/// return the count of alive cells around a given coordinate
		/// </summary>
		/// <param name="c">coordinate</param>
		/// <param name="range">count range</param>
		/// <returns></returns>
		public int CountAliveAdjacentCells(Coordinate c, int range)
		{
			int result = 0;
			Coordinate auxCoord = new Coordinate();
			//run on a range X range matrix
			for(int j = -range; j <= range; j++)
				for(int i = -range; i <= range; i++) {

					//in-buffer coordinate
					auxCoord.x = i + c.x;
					auxCoord.y = j + c.y;

					//If its not middle cell
					if(!( i == 0 && j == 0 ) &&

					   //If coordinates are outside bounds   (outside bounds cells are count as filled)
					   ( map.IsCoordinateOutsideBounds(auxCoord) ||

						 //if the cell is "true"
						 map[auxCoord] ))

						//add 1 to the counter
						result++;
				}
			//return counter
			return result;
		}

		public float GetMass(int range, Coordinate c)
		{
			Coordinate auxCoord = new Coordinate();
			float result = 0;
			//run on a range X range matrix
			for(int j = -range; j <= range; j++)
				for(int i = -range; i <= range; i++) {
					auxCoord.x = i + c.x;
					auxCoord.y = j + c.y;
					if(height.IsCoordinateWithinBounds(auxCoord) &&
						 !map[auxCoord]) {
						result += height[auxCoord];
					}
				}

			return result / 8f;
		}

		public void InvertHeight()
		{
			height.Write((cell) => 1f - cell);
		}

		/// <summary>
		/// Read the buffers and identify 'island' bubbles 
		/// </summary>
		/// <returns>size of the chamber</returns>
		public int RemoveDisconectedChambers()
		{
			//this reference has multiple purposes, it is generaly used as an auxiliar reference.
			Bubble mainChamber = null;

			//get the biggest bubble found (the main cavern)
			foreach(var hole in ListBubbles(false))
				if(mainChamber == null || mainChamber.count < hole.count)
					mainChamber = hole;

			//if there isn't a bubble (it might be full black)
			if(mainChamber != null)
				map.Write(mainChamber.cells, () => false, () => true);

			return ( mainChamber != null ) ? mainChamber.count : 0;
		}

		/// <summary>
		/// Recursive method that floods the cave to discover bubbles in it.
		/// </summary>
		public void Flood(ref bool[,] buffer, int x, int y, bool target)
		{
			//if this cell is valid and it's empty
			if(!( x < 0 || y < 0 || x >= buffer.GetLength(0) || y >= buffer.GetLength(0) ) && buffer[x, y] != target) {

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
		private void Flood(ref bool[,] buffer, Bubble hole, int x, int y, bool target)
		{
			//if this cell is valid and it's empty
			if(!( x < 0 || y < 0 || x >= buffer.GetLength(0) || y >= buffer.GetLength(0) ) &&
				buffer[x, y]!=target) {
				//write fill it
				buffer[x, y] = target;

				//add its coordinate to the hole's list
				hole.AddCell(x, y);

				//reverberate to adjacent cells
				Flood(ref buffer, hole, x + 1, y, target);
				Flood(ref buffer, hole, x - 1, y, target);
				Flood(ref buffer, hole, x, y + 1, target);
				Flood(ref buffer, hole, x, y - 1, target);
			}
		}

		/// <summary>
		/// Builds a list of bubbles
		/// </summary>
		/// <param name="target">if the bubbles are negative or positive</param>
		/// <returns>a list of bubbles</returns>
		private List<Bubble> ListBubbles(bool target)
		{
			//Initialize a bubble list to hold every bubble this map has
			List<Bubble> bubbles = new List<Bubble>();

			//this reference has multiple purposes, it is generaly used as an auxiliar reference.
			Bubble auxBubble = null;

			//extract buffer from data structure for more complex operations.
			bool[,] buffer = map.GetBufferCopy();

			//extract bubble from buffer
			Tools.Foreach2D(buffer, (Coordinate c, ref bool cell) => {
				if(cell == target) {
					auxBubble = new Bubble();
					Flood(ref buffer, auxBubble, c.x,c.y, !target);
					if(auxBubble.count > 0)
						bubbles.Add(auxBubble);
				}
			});

			return bubbles;
		}
	}
}
