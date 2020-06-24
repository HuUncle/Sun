using System;
using System.Collections.Generic;
using Sun.Core;

namespace Sun.EventBus.Extensions
{
    public class EventBusOption
    {
        public EventBusOption()
        {
            Extensions = new List<IOptionExtension>();
        }

        internal IList<IOptionExtension> Extensions { get; }

        /// <summary>
        /// Registers an extension that will be executed when building services.
        /// </summary>
        /// <param name="extension"> </param>
        public void RegisterExtension(IOptionExtension extension)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            Extensions.Add(extension);
        }
    }
}