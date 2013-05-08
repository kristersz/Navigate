using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Navigate.Services
{
    [Serializable]
    public class ServiceResult<T>
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets the service result messages. Messages will be populated in case if service had errors or warnings while processing request.
        /// </summary>
        public Message[] Messages { get; set; }

        public ServiceResult()
        {
        }
    }
}