using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;

using Microsoft.Extensions.Options;

namespace Discord.BotABordelV2.Services.Permissions;

public class PermissionsService(
    IOptions<PermissionsOptions> permissionsOpt,
    ILogger<PermissionsService> logger) : IPermissionsService
{
    public bool HasForceSkipPermission(IGuildUser user)
    {
        var privilegedIds = permissionsOpt.Value.PrivilegedRolesIds;

        // Check if user is in a privileged role
        var isPrivileged = (from role in user.RoleIds
                            join privilegedRole in privilegedIds
                            on role equals privilegedRole
                            select role)
                           .Any();

        if (!isPrivileged)
            return false;

        // Check if there is an exemption for the user to prevent him from force skipping
        var isForceExempted = permissionsOpt.Value.ForceSkipExemptionIds.Contains(user.Id);
        if (isForceExempted)
        {
            logger.LogInformation("User {name} had privileged role to force skip but was blocked by exemption", user.DisplayName);
            return false;
        }

        return true;
    }
}