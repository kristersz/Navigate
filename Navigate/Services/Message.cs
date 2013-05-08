using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Navigate.Services
{
    [Serializable]
    public class Message
    {
        /// <summary>
        /// Gets or sets the message level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the message severity
        /// </summary>
        public MessageSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the message text
        /// </summary>
        public string Text { get; set; }
    }
}