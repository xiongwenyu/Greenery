namespace Greenery.Event.ObjectModels
{
    /// <summary>
    /// 事件描述
    /// </summary>
    public class EventDescription
    {
        public EventDescription(params string[] descriptions)
        {
            Description = descriptions;
        }
        /// <summary>
        /// 事件描述详细信息
        /// </summary>
        public string[] Description { get; set; }

        public override string ToString()
        {
            return string.Join(PublisherConstant.Separator, Description);
        }
    }
}
