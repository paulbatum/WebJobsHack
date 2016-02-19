﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.ServiceBus
{
    // Helpers for doing general-purpose bindings. 
    internal class GenericBinder
    {


        // Bind an IAsyncCollector<TMessage> to a user parameter. 
        // This handles the various flavors of parameter types and will morph them to the connector.
        // parameter  - parameter being bound. 
        // TContext - helper object to pass to the binding the configuration state. This can point back to context like secrets, configuration, etc.
        // builder - function to create a new instance of the underlying Collector object to pass to the parameter. 
        //          This binder will wrap that in any adpaters to make it match the requested parameter type.
        public static IBinding BindCollector<TMessage, TContext>(
            ParameterInfo parameter,
            IConverterManager converterManager,
            TContext client,
            Func<TContext, ValueBindingContext, IFlushCollector<TMessage>> builder,
            string invokeString,
            Func<string, TContext> invokeStringBinder)
        {
            Type parameterType = parameter.ParameterType;

            Func<TContext, ValueBindingContext, IValueProvider> argumentBuilder = null;            

            if (parameterType.IsGenericType)
            {
                var genericType = parameterType.GetGenericTypeDefinition();
                var elementType = parameterType.GetGenericArguments()[0];

                if (genericType == typeof(IAsyncCollector<>))
                {
                    if (elementType == typeof(TMessage))
                    {
                        // Bind to IAsyncCollector<TMessage>. This is the "purest" binding, no adaption needed. 
                        argumentBuilder = (context, valueBindingContext) =>
                        {
                            IFlushCollector<TMessage> raw = builder(context, valueBindingContext);
                            return new CommonAsyncCollectorValueProvider<IAsyncCollector<TMessage>, TMessage>(raw, raw, invokeString);
                        };
                    }
                    else {
                        // Bind to IAsyncCollector<T>
                        // Get a converter from T to TMessage
                        argumentBuilder = DynamicInvokeBuildIAsyncCollectorArgument(elementType, converterManager, builder, invokeString);
                    }
                }
                else if (genericType == typeof(ICollector<>))
                {
                    if (elementType == typeof(TMessage))
                    {
                        // Bind to ICollector<TMessage> This just needs an Sync/Async wrapper
                        argumentBuilder = (context, valueBindingContext) =>
                        {
                            IFlushCollector<TMessage> raw = builder(context, valueBindingContext);
                            ICollector<TMessage> obj = new SyncAsyncCollectorAdapter<TMessage>(raw);
                            return new CommonAsyncCollectorValueProvider<ICollector<TMessage>, TMessage>(obj, raw, invokeString);
                        };
                    }
                    else
                    {
                        // Bind to ICollector<T>. 
                        // This needs both a conversion from T to TMessage and an Sync/Async wrapper
                        argumentBuilder = DynamicInvokeBuildICollectorArgument(elementType, converterManager, builder, invokeString);
                    }
                }
            }

            if (parameter.IsOut)
            {
                Type elementType = parameter.ParameterType.GetElementType();

                if (elementType.IsArray)
                {
                    if (elementType == typeof(TMessage[]))
                    {
                        argumentBuilder = (context, valueBindingContext) =>
                        {
                            IFlushCollector<TMessage> raw = builder(context, valueBindingContext);
                            return new OutArrayValueProvider<TMessage>(raw, invokeString);
                        };
                    }
                    else
                    {
                        // out TMessage[]
                        var e2 = elementType.GetElementType();
                        argumentBuilder = DynamicBuildOutArrayArgument(e2, converterManager, builder, invokeString);
                    }
                }
                else
                {
                    // Single enqueue
                    //    out TMessage
                    if (elementType == typeof(TMessage))
                    {
                        argumentBuilder = (context, valueBindingContext) =>
                        {
                            IFlushCollector<TMessage> raw = builder(context, valueBindingContext);
                            return new OutValueProvider<TMessage>(raw, invokeString);
                        };
                    }
                    else
                    {
                        // use JSon converter
                        // out T
                        argumentBuilder = DynamicInvokeBuildOutArgument(elementType, converterManager, builder, invokeString);
                    }
                }
            }

            if (argumentBuilder != null)
            {
                ParameterDescriptor param = new ParameterDescriptor {
                    Name = parameter.Name,
                    DisplayHints = new ParameterDisplayHints
                    {
                        Description = "output"
                    }
                };
                return new CommonCollectorBinding<TMessage, TContext>(client, argumentBuilder, param, invokeStringBinder);
            }

            string msg = string.Format(CultureInfo.CurrentCulture, "Can't bind to {0}.", parameter);
            throw new InvalidOperationException(msg);
        }


        // Helper to dynamically invoke BuildICollectorArgument with the proper generics
        static Func<TContext, ValueBindingContext, IValueProvider> DynamicBuildOutArrayArgument<TContext, TMessage>(
                Type typeMessageSrc,
                IConverterManager cm,
                Func<TContext, ValueBindingContext, IFlushCollector<TMessage>> builder,
                string invokeString)
        {
            var method = typeof(GenericBinder).GetMethod("BuildOutArrayArgument", BindingFlags.NonPublic | BindingFlags.Static);
            method = method.MakeGenericMethod(typeof(TContext), typeMessageSrc, typeof(TMessage));
            var argumentBuilder = (Func<TContext, ValueBindingContext, IValueProvider>)
            method.Invoke(null, new object[] { cm, builder, invokeString });
            return argumentBuilder;
        }

        static Func<TContext, ValueBindingContext, IValueProvider> BuildOutArrayArgument<TContext, TMessageSrc, TMessage>(
            IConverterManager cm,
            Func<TContext, ValueBindingContext, IFlushCollector<TMessage>> builder,
            string invokeString
            )
        {
            // Other 
            Func<TMessageSrc, TMessage> convert = cm.GetConverter<TMessageSrc, TMessage>();
            Func<TContext, ValueBindingContext, IValueProvider> argumentBuilder = (context, valueBindingContext) =>
            {
                IFlushCollector<TMessage> raw = builder(context, valueBindingContext);
                IFlushCollector<TMessageSrc> obj = new TypedAsyncCollectorAdapter<TMessageSrc, TMessage>(raw, convert);

                return new OutArrayValueProvider<TMessageSrc>(obj, invokeString);
            };
            return argumentBuilder;
        }


        // Helper to dynamically invoke BuildICollectorArgument with the proper generics
        static Func<TContext, ValueBindingContext, IValueProvider> DynamicInvokeBuildOutArgument<TContext, TMessage>(
                Type typeMessageSrc,
                IConverterManager cm,
                Func<TContext, ValueBindingContext, IFlushCollector<TMessage>> builder,
                string invokeString)
        {
            var method = typeof(GenericBinder).GetMethod("BuildOutArgument", BindingFlags.NonPublic | BindingFlags.Static);
            method = method.MakeGenericMethod(typeof(TContext), typeMessageSrc, typeof(TMessage));
            var argumentBuilder = (Func<TContext, ValueBindingContext, IValueProvider>)
            method.Invoke(null, new object[] { cm, builder, invokeString });
            return argumentBuilder;
        }

        static Func<TContext, ValueBindingContext, IValueProvider> BuildOutArgument<TContext, TMessageSrc, TMessage>(
            IConverterManager cm,
            Func<TContext, ValueBindingContext, IFlushCollector<TMessage>> builder,
            string invokeString
            )
        {
            // Other 
            Func<TMessageSrc, TMessage> convert = cm.GetConverter<TMessageSrc, TMessage>();
            Func<TContext, ValueBindingContext, IValueProvider> argumentBuilder = (context, valueBindingContext) =>
            {
                IFlushCollector<TMessage> raw = builder(context, valueBindingContext);
                IFlushCollector<TMessageSrc> obj = new TypedAsyncCollectorAdapter<TMessageSrc, TMessage>(raw, convert);
                return new OutValueProvider<TMessageSrc>(obj, invokeString);
            };
            return argumentBuilder;
        }



        // Helper to dynamically invoke BuildICollectorArgument with the proper generics
        static Func<TContext, ValueBindingContext, IValueProvider> DynamicInvokeBuildICollectorArgument<TContext, TMessage>(
                Type typeMessageSrc,
                IConverterManager cm,
                Func<TContext, ValueBindingContext, IFlushCollector<TMessage>> builder, 
                string invokeString)
        {
            var method = typeof(GenericBinder).GetMethod("BuildICollectorArgument", BindingFlags.NonPublic | BindingFlags.Static);
            method = method.MakeGenericMethod(typeof(TContext), typeMessageSrc, typeof(TMessage));
            var argumentBuilder = (Func<TContext, ValueBindingContext, IValueProvider>)
            method.Invoke(null, new object[] { cm, builder, invokeString });
            return argumentBuilder;
        }

        static Func<TContext, ValueBindingContext, IValueProvider> BuildICollectorArgument<TContext, TMessageSrc, TMessage>(
            IConverterManager cm,
            Func<TContext, ValueBindingContext, IFlushCollector<TMessage>> builder,
            string invokeString
            )
        {
            // Other 
            Func<TMessageSrc, TMessage> convert = cm.GetConverter<TMessageSrc, TMessage>();
            Func<TContext, ValueBindingContext, IValueProvider> argumentBuilder = (context, valueBindingContext) =>
            {
                IFlushCollector<TMessage> raw = builder(context, valueBindingContext);
                IAsyncCollector<TMessageSrc> obj = new TypedAsyncCollectorAdapter<TMessageSrc, TMessage>(raw, convert);
                ICollector<TMessageSrc> obj2 = new SyncAsyncCollectorAdapter<TMessageSrc>(obj);
                return new CommonAsyncCollectorValueProvider<ICollector<TMessageSrc>, TMessage>(obj2, raw, invokeString);
            };
            return argumentBuilder;
        }

        // Helper to dynamically invoke BuildIAsyncCollectorArgument with the proper generics
        static Func<TContext, ValueBindingContext, IValueProvider> DynamicInvokeBuildIAsyncCollectorArgument<TContext, TMessage>(
                Type typeMessageSrc,
                IConverterManager cm,
                Func<TContext, ValueBindingContext, IFlushCollector<TMessage>> builder,
                string invokeString)
        {
            var method = typeof(GenericBinder).GetMethod("BuildIAsyncCollectorArgument", BindingFlags.NonPublic | BindingFlags.Static);
            method = method.MakeGenericMethod(typeof(TContext), typeMessageSrc, typeof(TMessage));
            var argumentBuilder = (Func<TContext, ValueBindingContext, IValueProvider>)
            method.Invoke(null, new object[] { cm, builder, invokeString });
            return argumentBuilder;
        }

        // Helper to build an argument binder for IAsyncCollector<TMessageSrc>
        static Func<TContext, ValueBindingContext, IValueProvider> BuildIAsyncCollectorArgument<TContext, TMessageSrc, TMessage>(
            IConverterManager cm,
            Func<TContext, ValueBindingContext, IFlushCollector<TMessage>> builder,
            string invokeString
            )
        {
            Func<TMessageSrc, TMessage> convert = cm.GetConverter<TMessageSrc, TMessage>();
            Func<TContext, ValueBindingContext, IValueProvider> argumentBuilder = (context, valueBindingContext) =>
            {
                IFlushCollector<TMessage> raw = builder(context, valueBindingContext);
                IAsyncCollector<TMessageSrc> obj = new TypedAsyncCollectorAdapter<TMessageSrc, TMessage>(raw, convert);
                return new CommonAsyncCollectorValueProvider<IAsyncCollector<TMessageSrc>, TMessage>(obj, raw, invokeString);
            };
            return argumentBuilder;
        }

    
    }
}