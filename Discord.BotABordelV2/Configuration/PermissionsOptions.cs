namespace Discord.BotABordelV2.Configuration;

public class PermissionsOptions
{
    public List<ulong> PrivilegedRolesIds { get; set; } = new();

    public List<ulong> ForceSkipExemptionIds { get; set; } = new();
}