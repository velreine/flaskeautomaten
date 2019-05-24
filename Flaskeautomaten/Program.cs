using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flaskeautomaten
{
    class Program
    {

        private static Queue<string> _producerBuffer = new Queue<string>(200);
        private static readonly Queue<string> SodaBuffer = new Queue<string>(200);
        private static readonly Queue<string> BeerBuffer = new Queue<string>(200);
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
                        _producerBuffer.Enqueue($"soda {++sodanum}");
                        _producerBuffer.Enqueue($"beer {++beernum}");
                        Console.WriteLine("Putted a new soda and beer in the _producerBuffer");

                        Monitor.PulseAll(_producerBuffer);
                    }
                    
                    Thread.Sleep(500);
                }
            });


            Thread consumerThread = new Thread(() =>
            {
                // Start producer thread in consumer thread.



                while (true)
                {
                    lock (_producerBuffer)
                    {

                        while (_producerBuffer.Count == 0)
                        {
                            Console.WriteLine("Consumer Thread now waiting for a pulse from _producerBuffer.");
                            if (!producerThread.IsAlive) producerThread.Start();
                            Monitor.Wait(_producerBuffer);
                            


                            // When it is reacquired lets fill our soda & beer queue:
                            while (_producerBuffer.Count != 0)
                            {
                                var retrieved = _producerBuffer.Dequeue();
                                //Console.WriteLine("retrieved: " + retrieved);


                                if (retrieved.Contains("soda"))
                                {
                                    //SodaBuffer.Enqueue(retrieved);
                                    RefillSoda(new string[] { retrieved });
                                    Console.WriteLine(SodaBuffer.Dequeue());
                                }

                                if (retrieved.Contains("beer"))
                                {
                                    //BeerBuffer.Enqueue(retrieved);
                                    RefillBeer(new string[] { retrieved });
                                    Console.WriteLine(BeerBuffer.Dequeue());
                                }

                            }

                        }

                    }


                }


            });


            // Consumer Thread needs to be executed first.
            consumerThread.Start();
            //producerThread.Start();

            Console.ReadLine();


        }



        public static void RefillSoda(string[] sodas)
        {
            lock (SodaBuffer)
            {
                foreach (var soda in sodas)
                {
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
