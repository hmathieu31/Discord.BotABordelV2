namespace Discord.BotABordelV2.Configuration;

public class PermissionsOptions
{
    public List<ulong> ForceSkipExemptionIds { get; set; } = [];

    public List<ulong> PrivilegedRolesIds { get; set; } = [];
}