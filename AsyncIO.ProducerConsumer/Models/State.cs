// <copyright file="State.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncIO.ProducerConsumer.Models
{
    /// <summary>
    /// Provides consumer states.
    /// </summary>
    public enum State
    {
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
