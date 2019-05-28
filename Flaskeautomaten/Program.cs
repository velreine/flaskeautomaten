﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flaskeautomaten
{
    class Program
    {

        private static Queue<string> _producerBuffer = new Queue<string>(20);
        private static readonly Queue<string> SodaBuffer = new Queue<string>(10);
        private static readonly Queue<string> BeerBuffer = new Queue<string>(10);
        private static int sodanum = 0;
        private static int beernum = 0;


        static void Main(string[] args)
        {

            Thread producerThread = new Thread(() =>
            {
                while (true)
                {

                    lock (_producerBuffer)
                    {

                        if (_producerBuffer.Count < 20)
                        {
                            _producerBuffer.Enqueue($"soda {++sodanum}");
                            _producerBuffer.Enqueue($"beer {++beernum}");
                            Console.WriteLine("Putted a new soda and beer in the _producerBuffer");
                            Monitor.PulseAll(_producerBuffer);
                        }
                        else
                        {
                            Console.WriteLine("Buffer is full. Waiting for a pulse from consumer Thread!");
                            // Block producer until we have a pulse.
                            Monitor.Wait(_producerBuffer);
                        }




                    }

                    //Thread.Sleep(1000);
                }
            });


            Thread consumerThread = new Thread(() =>
            {

                while (true)
                {
                    lock (_producerBuffer)
                    {

                        while (_producerBuffer.Count == 0)
                        {
                            Console.WriteLine("Consumer Thread now waiting for a pulse from _producerBuffer.");
                            if (!producerThread.IsAlive) producerThread.Start();

                            // Wait for the producer to produce.
                            Monitor.Wait(_producerBuffer);
                        }

                        // Empty the producer buffer.
                        for (int i = 0; i < _producerBuffer.Count; i++)
                        {
                            var item = _producerBuffer.Dequeue();

                            if (item.Contains("soda"))
                            {
                                RefillSoda(new string[] { item });
                                lock (SodaBuffer)
                                {
                                    Console.WriteLine("Dequeued this item: " + SodaBuffer.Dequeue());
                                }

                            }

                            if (item.Contains("beer"))
                            {
                                RefillBeer(new string[] { item });
                                lock (BeerBuffer)
                                {
                                    Console.WriteLine("Dequeued this item: " + BeerBuffer.Dequeue());
                                }
                            }

                            // Notify the producer thread that it can start producing again.
                            Monitor.PulseAll(_producerBuffer);
                        }




                        #region a
                        /*// When it is reacquired lets fill our soda & beer queue:
                        while (_producerBuffer.Count != 0)
                        {
                            var retrieved = _producerBuffer.Dequeue();
                            //Console.WriteLine("retrieved: " + retrieved);


                            if (retrieved.Contains("soda"))
                            {
                                //SodaBuffer.Enqueue(retrieved);
                                RefillSoda(new string[] { retrieved });
                                Console.WriteLine("Dequeued this item: " + SodaBuffer.Dequeue());
                            }

                            if (retrieved.Contains("beer"))
                            {
                                //BeerBuffer.Enqueue(retrieved);
                                RefillBeer(new string[] { retrieved });
                                Console.WriteLine("Dequeued this item: " + BeerBuffer.Dequeue());
                            }

                        }*/
                        #endregion




                    }

                    // Slow down consumption.
                    Thread.Sleep(6000);
                }


            });


            // Consumer Thread needs to be executed first.
            consumerThread.Start();


            Console.ReadLine();


        }



        public static void RefillSoda(string[] sodas)
        {
            lock (SodaBuffer)
            {
                foreach (var soda in sodas)
                {
                    Console.WriteLine($"Added soda: {soda} to soda queue.");
                    SodaBuffer.Enqueue(soda);
                }

                Monitor.PulseAll(SodaBuffer);
            }
        }

        public static void RefillBeer(string[] beers)
        {
            lock (BeerBuffer)
            {
                foreach (var beer in beers)
                {
                    Console.WriteLine($"Added beer: {beer} to beer queue.");
                    BeerBuffer.Enqueue(beer);
                }

                Monitor.Pulse(BeerBuffer);
            }
        }




        public string GetSoda()
        {
            lock (SodaBuffer)
            {

                while (SodaBuffer.Count == 0)
                {
                    Monitor.Wait(SodaBuffer);
                }

                return SodaBuffer.Dequeue();


            }
        }

        public string GetBeer()
        {

            lock (BeerBuffer)
            {
                while (BeerBuffer.Count == 0)
                {
                    Monitor.Wait(BeerBuffer);
                }

                return BeerBuffer.Dequeue();
            }

        }





    }
}
