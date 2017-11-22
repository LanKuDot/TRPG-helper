using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace discordTRPGHelper
{
    public class RequireGMPermissionAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as SocketGuildUser;
            // Get the user roles
            IEnumerator<SocketRole> roles = user.Roles.GetEnumerator();
            roles.MoveNext();   // Move to the first item
            roles.MoveNext();   // Ignore "@everyone"

            if (roles.Current.Name == "GM")
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("You must be the GM to run this command.");
        }
    }
}