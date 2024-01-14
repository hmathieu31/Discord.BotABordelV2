namespace Discord.BotABordelV2.Exceptions;

public class MediaExceptions
{
    public class InvalidChannelTypeException : Exception
    {
        public InvalidChannelTypeException()
        { }

        public InvalidChannelTypeException(string message) : base(message)
        {
        }

        public InvalidChannelTypeException(string message, Exception inner) : base(message, inner)
        {
        }

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

        public ChannelType RequiredChannelType { get; }
    }

    public class NullChannelConnectionException : Exception
    {
        public NullChannelConnectionException()
        { }

        public NullChannelConnectionException(string message) : base(message)
        {
        }

        public NullChannelConnectionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}