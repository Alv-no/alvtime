using System;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Task = System.Threading.Tasks.Task;

namespace AlvTimeWebApi.Infrastructure;

public class GraphService(GraphServiceClient graphServiceClient, AlvTime_dbContext context)
{
    public async Task<string> GetObjectIdByEmail(string email)
    {
        try
        {
            var users = await graphServiceClient.Users
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["id"];
                    requestConfiguration.QueryParameters.Filter = $"mail eq '{email}' or userPrincipalName eq '{email}'";
                });

            if (users?.Value?.FirstOrDefault()?.Id == null)
            {
                return "";
            }

            return users.Value.First().Id;
        }
        catch (ServiceException ex)
        {
            throw new ServiceException($"Failed to fetch object ID: {ex.Message}");
        }
    }

    public async Task SetUserOid()
    {
        var users = await context.User.ToListAsync();

        foreach (var user in users)
        {
            var objectId = await GetObjectIdByEmail(user.Email);
            if (objectId != null)
            {
                user.Oid = objectId;
            }
            else
            {
                Console.WriteLine($"User with email {user.Email} not found in Entra ID");
            }

            await context.SaveChangesAsync();
        }
    }
}