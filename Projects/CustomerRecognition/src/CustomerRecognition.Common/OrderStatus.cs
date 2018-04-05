using System.Runtime.Serialization;

namespace CustomerRecognition.Common
{
    public enum OrderStatus
    {
        [EnumMember(Value = "pending")]
        Pending,
        [EnumMember(Value = "ready")]
        Ready,
        [EnumMember(Value = "complete")]
        Complete
    }
}
