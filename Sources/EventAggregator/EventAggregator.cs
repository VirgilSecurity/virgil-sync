namespace Infrastructure.Messaging {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    
    /// <summary>
    ///   Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public class EventAggregator : IEventAggregator {
        readonly List<Handler> handlers = new List<Handler>();

        /// <summary>
        ///   The default thread marshaller used for publication;
        /// </summary>
        public static Action<System.Action> DefaultPublicationThreadMarshaller = action => action();

        /// <summary>
        /// Processing of handler results on publication thread.
        /// </summary>
        public static Action<object, object> HandlerResultProcessing = (target, result) => { };

        /// <summary>
        ///   Initializes a new instance of the <see cref = "EventAggregator" /> class.
        /// </summary>
        public EventAggregator() {
            this.PublicationThreadMarshaller = DefaultPublicationThreadMarshaller;
        }

        /// <summary>
        ///   Gets or sets the default publication thread marshaller.
        /// </summary>
        /// <value>
        ///   The default publication thread marshaller.
        /// </value>
        public Action<System.Action> PublicationThreadMarshaller { get; set; }

        /// <summary>
        /// Searches the subscribed handlers to check if we have a handler for
        /// the message type supplied.
        /// </summary>
        /// <param name="messageType">The message type to check with</param>
        /// <returns>True if any handler is found, false if not.</returns>
        public bool HandlerExistsFor(Type messageType) {
                return this.handlers.Any(handler => handler.Handles(messageType) & !handler.IsDead);
        }

        /// <summary>
        ///   Subscribes an instance to all events declared through implementations of <see cref = "IHandle{T}" />
        /// </summary>
        /// <param name = "subscriber">The instance to subscribe for event publication.</param>
        public virtual void Subscribe(object subscriber) {
            if (subscriber == null) {
                throw new ArgumentNullException("subscriber");
            }
            lock(this.handlers) {
                if (this.handlers.Any(x => x.Matches(subscriber))) {
                    return;
                }

                this.handlers.Add(new Handler(subscriber));
            }
        }

        /// <summary>
        ///   Unsubscribes the instance from all events.
        /// </summary>
        /// <param name = "subscriber">The instance to unsubscribe.</param>
        public virtual void Unsubscribe(object subscriber) {
            if (subscriber == null) {
                throw new ArgumentNullException("subscriber");
            }
            lock(this.handlers) {
                var found = this.handlers.FirstOrDefault(x => x.Matches(subscriber));

                if (found != null) {
                    this.handlers.Remove(found);
                }
            }
        }

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <remarks>
        ///   Does not marshall the the publication to any special thread by default.
        /// </remarks>
        public virtual void Publish(object message) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }
            this.Publish(message, this.PublicationThreadMarshaller);
        }

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <param name = "marshal">Allows the publisher to provide a custom thread marshaller for the message publication.</param>
        public virtual void Publish(object message, Action<Action> marshal) {
            if (message == null){
                throw new ArgumentNullException("message");
            }
            if (marshal == null) {
                throw new ArgumentNullException("marshal");
            }

            Handler[] toNotify;
            lock (this.handlers) {
                toNotify = this.handlers.ToArray();
            }

            marshal(() => {
                var messageType = message.GetType();

                var dead = toNotify
                    .Where(handler => !handler.Handle(messageType, message))
                    .ToList();

                if(dead.Any()) {
                    lock(this.handlers) {
                        dead.Apply(x => this.handlers.Remove(x));
                    }
                }
            });
        }

        class Handler {
            readonly WeakReference reference;
            readonly Dictionary<Type, MethodInfo> supportedHandlers = new Dictionary<Type, MethodInfo>();

            public bool IsDead {
                get { return this.reference.Target == null; }
            }

            public Handler(object handler) {
                this.reference = new WeakReference(handler);

                var interfaces = handler.GetType().GetInterfaces()
                    .Where(x => typeof(IHandle).IsAssignableFrom(x) && x.IsGenericType);

                foreach(var @interface in interfaces) {
                    var type = @interface.GetGenericArguments()[0];
                    var method = @interface.GetMethod("Handle");
                    this.supportedHandlers[type] = method;
                }

            }

            public bool Matches(object instance) {
                return this.reference.Target == instance;
            }

            public bool Handle(Type messageType, object message) {
                var target = this.reference.Target;
                if (target == null) {
                    return false;
                }

                foreach(var pair in this.supportedHandlers) {
                    if(pair.Key.IsAssignableFrom(messageType)) {
                        var result = pair.Value.Invoke(target, new[] { message });
                        if (result != null) {
                            HandlerResultProcessing(target, result);
                        }
                    }
                }
                
                return true;
            }

            public bool Handles(Type messageType) {
                return this.supportedHandlers.Any(pair => pair.Key.IsAssignableFrom(messageType));
            }
        }
    }
}
