///-----------------------------------------------------------------
///   Struct:         SwapChain
///   Description:    SwapChain Design patern: 
///                   It contains 2 buffers holding the information of the map being generated.
///                   These buffers have a swapping mechanism to auxiliate the process of the cellular automata.
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using System.Collections.Generic;

namespace CaveMapGenerator
{
    /// <summary>
    /// This struct holds a simple structure of buffers, 
    /// these buffers are the canvas for the celular automata
    /// </summary>
    public struct SwapChain
    {
        /// <summary>
        /// The buffer that will have its information written
        /// </summary>
        public bool[,] WriteBuffer { get; private set; }

        /// <summary>
        /// This buffer holds the information of the previous iteration
        /// </summary>
        public bool[,] ReadBuffer { get; private set; }

        /// <summary>
        /// Initialize a swapchain struct
        /// </summary>
        /// <param name="size">The size of this buffer</param>
        /// <param name="newBuffer">
        /// if desired a new buffer could be passed as reference,
        /// if that happens the swap chain starts its new buffer with that information</param>
        public SwapChain(int size, bool[,] newBuffer = null)
        {
            WriteBuffer = new bool[size, size];
            ReadBuffer = newBuffer ?? new bool[size, size];
        }

        /// <summary>
        /// Swap the 2 buffers around
        /// </summary>
        public void FlipBuffers()
        {
            bool[,] auxiliarBuffer = WriteBuffer;
            WriteBuffer = ReadBuffer;
            ReadBuffer = auxiliarBuffer;
        }
    }
    public struct Coordinate
    {
        public int x, y;
        public Coordinate(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Hole
    {
        public int count;
        public List<Coordinate> holeCells;

        public Hole()
        {
            holeCells = new List<Coordinate>();
            count = 0;
        }
        public void AddCell(int x, int y)
        {
            holeCells.Add(new Coordinate(x, y));
            count++;
        }
    }
}