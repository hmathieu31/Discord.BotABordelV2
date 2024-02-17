namespace Discord.BotABordelV2.Configuration;

public class PermissionsOptions
{
    public const string ConfigSectionName = "Permissions";

    public List<ulong> ForceSkipExemptionIds { get; set; } = [];

    public List<ulong> PrivilegedRolesIds { get; set; } = [];
}