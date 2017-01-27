using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(GameSystem))]
public class PubSubHub : MonoBehaviour {

    internal class Handler {
        public Delegate action {
            get; set;
        }

        public WeakReference sender {
            get; set;
        }

        public Type type {
            get; set;
        }
    }

    internal List<Handler> handlers = new List<Handler>();
    internal object locker = new object();

    public void Subscribe<T>(object sender, Action<T> handler)
    {
        var item = new Handler {
            action = handler,
            sender = new WeakReference(sender),
            type = typeof(T)
        };

        lock (locker)
            handlers.Add(item);
    }

    public void Unsubscribe<T>(object sender, Action<T> handler = null)
    {
        lock (locker) {
            var query = handlers.Where(a => !a.sender.IsAlive || (a.sender.Target.Equals(sender) && a.type == typeof(T)));

            if (handler != null)
                query = query.Where(a => a.action.Equals(handler));

            foreach (var h in query.ToList())
                this.handlers.Remove(h);
        }
    }

    public void Publish<T>(object sender, T data = default(T))
    {
        var handlerList = new List<Handler>(handlers.Count);
        var handlersToRemoveList = new List<Handler>(handlers.Count);

        lock (locker) {
            foreach (var handler in handlers) {
                if (!handler.sender.IsAlive)
                    handlersToRemoveList.Add(handler);

                else if (handler.type.IsAssignableFrom(typeof(T)))
                    handlerList.Add(handler);
            }

            foreach (var l in handlersToRemoveList)
                handlers.Remove(l);
        }

        foreach (var l in handlerList)
            ((Action<T>)l.action)(data);
    }
}