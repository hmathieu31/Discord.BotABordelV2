using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.BotABordelV2.Exceptions;
public class MediaExceptions
{

    [Serializable]
    public class InvalidChannelTypeException : Exception
    {
        public InvalidChannelTypeException() { }
        public InvalidChannelTypeException(string message) : base(message) { }
        public InvalidChannelTypeException(string message, Exception inner) : base(message, inner) { }
        public InvalidChannelTypeException(ChannelType requiredChannelType) :
            base($"Channel does not match required {requiredChannelType} type channel")
        {
            RequiredChannelType = requiredChannelType;
        }
        public InvalidChannelTypeException(ChannelType requiredChannelType, Exception inner) :
            base($"Channel does not match required {requiredChannelType} type channel", inner)
        {
            RequiredChannelType = requiredChannelType;
        }
        protected InvalidChannelTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public ChannelType RequiredChannelType { get; }
    }
}
