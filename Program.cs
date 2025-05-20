using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace WindowsFormsApp1
{
    static class Program
    {
        static MySemaphore semaphore = new MySemaphore(initialCount: 3, maxCount: 3); //example usage

        static void Worker(object id)
        {
            Console.WriteLine($"Thread {id} waiting...");
            semaphore.WaitOne();
            Console.WriteLine($"Thread {id} entered critical section.");
            Thread.Sleep(2000); // simulate work
            Console.WriteLine($"Thread {id} exiting critical section.");
            semaphore.Release();
        }

        static void Main()
        {
            for (int i = 1; i <= 10; i++)
            {
                Thread t = new Thread(Worker);
                t.Start(i);
            }

            Console.ReadLine();
        }
    }
}
public class MySemaphore //implementing a semaphore as a mutex
{
    private int count; //the initial count
    private int maxCount; //the maximum count
    private Mutex mutex = new Mutex(); //using a mutex for adjusting the count, which mimics the behaviour of a semaphore

    public MySemaphore(int initialCount, int maxCount = int.MaxValue)
    {
        this.count = initialCount; //basic constructor
        this.maxCount = maxCount;
    }

    public bool WaitOne()
    {
        while (true) //try to insert the thread to the critical section, only if the counter is not 0. If it is, make the thread sleep for a 0.1s and try again
        {
            mutex.WaitOne(); //start of critical section, checking the counter and adjusting it if needed
            if (count > 0)
            {
                count--;
                mutex.ReleaseMutex();
                break;
            }
            mutex.ReleaseMutex(); //releasing the mutex
            Thread.Sleep(100); // to prevent tight spin loop
        }
        return false;
    }

    public bool Release(int num = 1)
    {
        mutex.WaitOne(); //start of critical section, checking and adjusting the counter

        if (count + num > maxCount) //incase the current count plus num is higher than the capacity, release mutex and throw exception
        {
            mutex.ReleaseMutex();
            throw new InvalidOperationException("Semaphore count exceeded max");
        }

        count += num; //in critical zone, so we update the count to be + num as we are releasing num slots

        
        mutex.ReleaseMutex(); //releasing the mutex to allow other threads to access the semaphore

        return false; // Return value not used, kept for signature consistency
    }

}
