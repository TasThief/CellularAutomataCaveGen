///-----------------------------------------------------------------
///   Class:          Generator
///   Description:    It uses cellular automata to generate a cave complex
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

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
        /// Fill a buffer with random noise
        /// </summary>
        public void NewMap(int size, float initialDensity)
        {
            //reset the swapchain if needed
            if (size != Size)
                swapChain = new SwapChain(size);

            //fill the buffers with random noise
            Tools.Foreach2D(swapChain.WriteBuffer, (ref bool cell) =>
                cell = Random.value < initialDensity);

            //flip the buffer to have the written buffer on the read part
            swapChain.FlipBuffers();
        }

        /// <summary>
        /// Counts how much cells around the given coodinate are alive
        /// </summary>
        /// <returns>the ammount of true cells</returns>
        private int CountAliveAdjacentCells(int cellX, int cellY)
        {
            int result = 0, x, y;

            for (int j = -1; j < 2; j++)
                for (int i = -1; i < 2; i++)
                {
                    x = i + cellX;
                    y = j + cellY;

                    if (!(i == 0 && j == 0) &&
                       ((x < 0 || y < 0 || x >= Size || y >= Size) ||
                         swapChain.WriteBuffer[x, y]))
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
                Map[x, y] = (cell) ?
                     count >= minThreshold:
                     count > maxThreshold;
            });
        }

        /// <summary>
        /// Recursive method that floods the cave to discover holes in it.
        /// </summary>
        public void Flood(int x, int y)
        {
            //if this cell is valid and it's empty
            if (!(x < 0 || y < 0 || x >= Size || y >= Size) &&
                !swapChain.ReadBuffer[x, y])
            {
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
            if (!(x < 0 || y < 0 || x >= Size || y >= Size) &&
                !swapChain.WriteBuffer[x, y])
            {
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
            Tools.Foreach2D(swapChain.WriteBuffer, (int x, int y, ref bool cell) =>
            {

                //if that cell is is open
                if (!cell)
                {
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
            foreach (var hole in holes)
                if (auxiliarHoleReference == null || auxiliarHoleReference.count < hole.count)
                    auxiliarHoleReference = hole;

            //if there's a hole in the entire map (it might be full black
            if (auxiliarHoleReference != null)
            {
                //set the entire buffer to be black
                Tools.Foreach2D(swapChain.WriteBuffer, (ref bool cell) => cell = true);

                //for each cell recorded at the hole object, paint the buffer white
                foreach (Coordinate coordinate in auxiliarHoleReference.holeCells)
                    swapChain.WriteBuffer[coordinate.x, coordinate.y] = false;
            }

            //Flip the buffers
            swapChain.FlipBuffers();

            return (auxiliarHoleReference != null) ? auxiliarHoleReference.count : 0;
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
            for (int i = 0; i < refinementSteps; i++)
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
            do
            {
                //add one trial to the safe lock
                safeLock++;

                //initialize the map
                NewMap(mapSize, initialDensity);

                //refine it some times
                for (int i = 0; i < refinementSteps; i++)
                    Refine(minThreshold, maxThreshold);

                //auto identify holes
                size = AutoIdentifyHoles();

                //write the debug log
                debugMessage += "Atempt number " + safeLock + ", Map size = " + size + " cells\n";

                //check is the generated map is fit
            } while ((size < minCaveSize || size > maxCaveSize) && safeLock < 10);

            //if the map is not ideal
            if (safeLock >= 10)
                //send an warning
                Debug.LogWarning("Map generated is not ideal\n" + debugMessage);
            else
                //if its ideal send a ok message
                Debug.Log("Map with accepted size generated\n" + debugMessage);

        }
        
    }
}