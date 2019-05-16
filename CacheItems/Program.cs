using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace CacheItems
{
    public class CacheItem
    {
        public int Id { get; set; }
        public CacheItem(int id)
        {
            this.Id = id;
        }
    }

    public class Cache
    {
        public IObservable<IObservable<CacheItem>> GetSubscriptionUpdates(int issueid)
        {
            var subscription = Observable.Create((IObserver<IObservable<CacheItem>> observer) =>
            {
                int i = 0;
                while(i < 20)
                {
                    observer.OnNext(Observable.Return(new CacheItem(i)));

                    try
                    {
                        if(i % 4 == 0)
                        {
                            throw new System.ArgumentException("Parameter cannot be null", "original");
                        }
                        observer.OnNext(Observable.Return(new CacheItem(i+1)));
                    }
                    catch
                    {
                        observer.OnNext(Observable.Return(new CacheItem(999)));
                    }

                    Thread.Sleep(100);
                    i += 2;
                }
                observer.OnCompleted();
                return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
            }).Publish().RefCount();

            subscription.Subscribe();

            return subscription;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var cache = new Cache();
            var subscription1 = cache.GetSubscriptionUpdates(1);
            var times5 = subscription1.SelectMany(x => x);


            Console.WriteLine("Sub1");
            subscription1
                .SubscribeOn(Scheduler.Default)
                .SelectMany(x => x)
                .Subscribe(x =>
            {
                Console.WriteLine($"Client 1 => {x.Id}");
            });

            Console.WriteLine("Sub2");
            subscription1
                .SubscribeOn(Scheduler.Default)
                .SelectMany(x => x)
                .Subscribe(x =>
            {
                Console.WriteLine($"Client 2 => {x.Id}");
            });

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
