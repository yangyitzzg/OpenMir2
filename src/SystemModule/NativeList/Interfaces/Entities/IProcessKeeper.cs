﻿using System;
using System.Diagnostics;
using SystemModule.NativeList.Interfaces.Shared;

namespace SystemModule.NativeList.Interfaces.Entities
{
    /// <summary>
    /// This interface was created for the objects that work as wrapper for another application.
    /// </summary>
    public interface IProcessKeeper : IDisposable, IDisposeIndication
    {
        /// <summary>
        /// Associated process.
        /// </summary>
        /// <remarks>
        /// Please do not turn off this: <see cref="Process.EnableRaisingEvents"/>
        /// </remarks>
        Process AssociatedProcess { get; }
    }
}