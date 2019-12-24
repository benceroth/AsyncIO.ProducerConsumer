// <copyright file="ConsumerState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Provides consumer states.
    /// </summary>
    public enum ConsumerState
    {
        /// <summary>
        /// Consumer is waiting for item.
        /// </summary>
        Waiting,

        /// <summary>
        /// Consumer is consuming an item.
        /// </summary>
        Busy,

        /// <summary>
        /// Consumer has been completed.
        /// </summary>
        Completed,
    }
}
