using System;

namespace OmegleAPI
{
    internal static class URLs
    {
        /// <summary>
        /// Base URL for Omegle
        /// </summary>
        public const String Omegle = "omegle.com";
        /// <summary>
        /// Status URL. Used to get a List of available Servers
        /// </summary>
        public const String Status = "https://" + Omegle + "/status";
        /// <summary>
        /// URL for Starting a Chat. Usage: <c>[0] + Server + [1]</c>
        /// </summary>
        public static readonly String[] Connect = { "https://", "/start" };
        /// <summary>
        /// URL for Getting Events. Usage: <c>[0] + Server + [1]</c>
        /// </summary>
        public static readonly String[] Events = { "https://", "/events" };
        /// <summary>
        /// URL to start being displayed as "Typing". Usage: <c>[0] + Server + [1]</c>
        /// </summary>
        public static readonly String[] TypingStart = { "https://", "/typing" };
        /// <summary>
        /// URL to stop being displayed as "Typing". Usage: <c>[0] + Server + [1]</c>
        /// </summary>
        public static readonly String[] TypingStop = { "https://", "/stoppedtyping" };
        /// <summary>
        /// URL for disconnecting. Usage: <c>[0] + Server + [1]</c>
        /// </summary>
        public static readonly String[] Disconnect = { "https://", "/disconnect" };
        /// <summary>
        /// URL for sending a Message. Usage: <c>[0] + Server + [1]</c>
        /// </summary>
        public static readonly String[] Send = { "https://", "/send" };
    }
}
