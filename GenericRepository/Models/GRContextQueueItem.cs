using GenericRepository.Interfaces;

namespace GenericRepository.Models
{
    public enum GRContextQueueAction
    {
        Insert,
        Update,
        Delete
    }

    public class GRContextQueueItem
    {
        public GRContextQueueAction Action { get; set; }
        public IGRUpdatable Item { get; set; }
    }
}
