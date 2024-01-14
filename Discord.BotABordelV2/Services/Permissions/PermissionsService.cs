using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Constants;
using Discord.BotABordelV2.Interfaces;
using Discord.Commands;

using Microsoft.Extensions.Options;

using System.Runtime.InteropServices;

namespace Discord.BotABordelV2.Services.Permissions;

public class PermissionsService : IPermissionsService
{
    private readonly ILogger<PermissionsService> _logger;
    private readonly IOptions<PermissionsOptions> _permissionsOpt;

    public PermissionsService(
        IOptions<PermissionsOptions> permissionsOpt,
        ILogger<PermissionsService> logger)
    {
        _permissionsOpt = permissionsOpt;
        _logger = logger;
    }

    public bool HasForceSkipPermission(IGuildUser user)
    {
        var privilegedIds = _permissionsOpt.Value.PrivilegedRolesIds;

        // Check if user is in a privileged role
        var isPrivileged = (from role in user.RoleIds
                            join privilegedRole in privilegedIds
                            on role equals privilegedRole
                            select role)
                           .Any();

        if (!isPrivileged)
            return false;

        // Check if there is an exemption for the user to prevent him from force skipping
        var isForceExempted = _permissionsOpt.Value.ForceSkipExemptionIds.Contains(user.Id);
        if (isForceExempted)
        {
            _logger.LogInformation("User {name} had privileged role to force skip but was blocked by exemption", user.DisplayName);
            return false;
        }

        return true;
    }
}