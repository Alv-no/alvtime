using System;
using System.Threading.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Task = System.Threading.Tasks.Task;

namespace AlvTimeWebApi.Infrastructure;

public class GraphService(GraphServiceClient graphServiceClient, AlvTime_dbContext context)
{
    public async Task<string> GetObjectIdByEmail(string email)
    {
        var users = await graphServiceClient.Users
            .GetAsync(requestConfiguration => 
            {
                requestConfiguration.QueryParameters.Select = ["id"];
                requestConfiguration.QueryParameters.Filter = $"mail eq '{email}' or userPrincipalName eq '{email}'";
            });

            Console.WriteLine("hei");
            // var user = users.FirstOrDefault();
            // if (user == null)
            // {
            //     return new Error(ErrorCodes.MissingEntity, $"User with email {email} not found in Entra ID");
            // }

            return "hehe";
        // catch (ServiceException ex)
        // {
        //     return new Error(ErrorCodes.AuthorizationError, $"Failed to fetch object ID: {ex.Message}");
        // }

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