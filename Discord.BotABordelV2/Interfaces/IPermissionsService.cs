namespace Discord.BotABordelV2.Interfaces;

public interface IPermissionsService
{
    public bool HasForceSkipPermission(IGuildUser user);
}