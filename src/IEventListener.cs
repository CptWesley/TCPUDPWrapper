namespace TCPUDPWrapper
{
    /// <summary>
    /// Event listener interface.
    /// </summary>
    public interface IEventListener
    {
        /// <summary>
        /// Detaches the event listener from the object it's attached to.
        /// </summary>
        void Detach();
    }
}
