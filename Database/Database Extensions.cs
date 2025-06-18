namespace MainBot.Database;

internal static class DatabaseExtensions
{
    internal static async Task<int> ApplyChangesAsync(this DatabaseContext database, object? entity = null)
    {
        try
        {
            if (entity is not null)
            {
                database.Update(entity);
            }

            return await database.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await ex.LogErrorAsync(database);
            return 0;
        }
    }

    internal static async Task LogErrorAsync(this Exception e, DatabaseContext database, string? extraInformation = null)
    {
        try
        {
            //dont log specific errors
            var entry = new Models.Logs.ErrorLog
            {
                errorTime = DateTime.UtcNow,
                source = e.Source,
                message = e.Message,
                stackTrace = e.StackTrace,
                extraInformation = extraInformation
            };
            await database.Errors.AddAsync(entry);
            await database.ApplyChangesAsync();
        }
        catch
        {
            Console.WriteLine($"{e.Source} | {e.Message}\n{e.StackTrace}");
        }
    }
}
