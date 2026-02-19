using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Tic_Tac_Toe.datasource.dbcontext;

public static class DatabaseInitializer
{
    public static void Initialize(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            if (!context.Database.CanConnect())
            {
                logger.LogError("Не удается подключиться к базе данных. Проверьте строку подключения и убедитесь, что PostgreSQL запущен.");
                throw new InvalidOperationException("Не удается подключиться к базе данных PostgreSQL");
            }

            logger.LogInformation("Подключение к базе данных установлено");

            bool tablesExist = false;
            try
            {
                var testQuery = context.Database.ExecuteSqlRaw("SELECT 1 FROM \"Users\" LIMIT 1");
                tablesExist = true;
                logger.LogInformation("Таблица Users существует");
            }
            catch
            {
                tablesExist = false;
                logger.LogInformation("Таблица Users не найдена, нужно создать таблицы");
            }

            if (!tablesExist)
            {
                logger.LogInformation("Таблицы не найдены, создаем базу данных...");
                
                try
                {
                    var pendingMigrations = context.Database.GetPendingMigrations().ToList();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Найдены ожидающие миграции: {Count}", pendingMigrations.Count);
                        context.Database.Migrate();
                        logger.LogInformation("Миграции применены успешно");
                    }
                    else
                    {
                        logger.LogInformation("Миграций нет, создаем базу данных через EnsureCreated()");
                        context.Database.EnsureCreated();
                        logger.LogInformation("База данных создана успешно через EnsureCreated()");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при создании базы данных: {Message}", ex.Message);
                    throw;
                }
            }
            else
            {
                logger.LogInformation("Таблицы уже существуют");
            }

            logger.LogInformation("Инициализация базы данных завершена");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Критическая ошибка при инициализации базы данных: {Message}", ex.Message);
            throw;
        }
    }
}
