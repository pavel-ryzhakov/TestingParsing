using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Npgsql;
using Microsoft.Extensions.Hosting;

namespace ConsoleParser
{
    //public class PassportParser
    //{
    //private readonly IDbContextFactory<PassportDbContext> _dbContextFactory;
    //private const int BatchSize = 10000;

    //public PassportParser(IDbContextFactory<PassportDbContext> dbContextFactory)
    //{
    //    _dbContextFactory = dbContextFactory;
    //}

    //public async Task ProcessPassportsAsync(string filePath)
    //{
    //    // Создаем блок 
    //    var readBlock = new TransformBlock<string, (int passp_series, int passp_number)>(
    //        line =>
    //        {
    //            var values = line.Split(',');
    //            // Парс серии и номера
    //            if (values[0].Length == 4 && values[1].Length == 6)
    //            {
    //                // Попробуем конвертировать в целочисленные значения
    //                if (int.TryParse(values[0], out var passpSeries) && int.TryParse(values[1], out var passpNumber))
    //                {
    //                    return (passpSeries, passpNumber);
    //                }
    //            }
    //            return (0, 0); // пустые для невалидных данных
    //        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount });

    //    var insertBlock = new ActionBlock<(int passp_series, int passp_number)[]>(async batch =>
    //    {
    //        await InsertBatchAsync(batch);
    //    }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, }); // Параллельная вставка данных

    //    var batchBlock = new BatchBlock<(int passp_series, int passp_number)>(BatchSize);

    //    // Соединение блоков
    //    readBlock.LinkTo(batchBlock);
    //    batchBlock.LinkTo(insertBlock, new DataflowLinkOptions { PropagateCompletion = true });

    //    // Чтение файла и отправка данных в блоки
    //    using (var reader = new StreamReader(filePath))
    //    {
    //        while (!reader.EndOfStream)
    //        {
    //            var line = await reader.ReadLineAsync();
    //            if (!string.IsNullOrWhiteSpace(line))
    //            {
    //                await readBlock.SendAsync(line);
    //            }
    //        }
    //    }

    //    // Завершаем блоки после чтения
    //    readBlock.Complete();
    //    await readBlock.Completion;
    //    batchBlock.Complete();
    //    await insertBlock.Completion;
    //}

    //private async Task InsertBatchAsync((int passp_series, int passp_number)[] batch)
    //{

    //    using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
    //    using (var conn = new NpgsqlConnection(dbContext.Database.GetDbConnection().ConnectionString))
    //    {
    //        await conn.OpenAsync();
    //        //SQL COPY https://www.postgresql.org/docs/current/sql-copy.html
    //        using (var writer = conn.BeginBinaryImport("COPY passports (passp_series, passp_number) FROM STDIN (FORMAT BINARY)"))
    //        {
    //            foreach (var item in batch)
    //            {
    //                if (item.passp_series != 0 && item.passp_number != 0)
    //                {
    //                    writer.StartRow();
    //                    writer.Write(item.passp_series);
    //                    writer.Write(item.passp_number);
    //                }
    //            }
    //            writer.Complete();
    //        }

    //    }
    //}

    //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //{
    //    await ProcessPassportsAsync(@"C:\Users\User\Desktop\Data.csv");

    //}
    //}

    public class PassportParser
    {
        private readonly IDbContextFactory<PassportDbContext> _dbContextFactory;
        private const int BatchSize = 10000; // Размер батча для вставки данных

        public PassportParser(IDbContextFactory<PassportDbContext> _dbContextFactory)
        {
            this._dbContextFactory = _dbContextFactory;
        }

        public async Task ProcessPassportsAsync(string filePath)
        {
            var readBlock = new TransformBlock<string, (int passpSeries, int passpNumber)?>(
                line =>
                {
                    var values = line.Split(',');

                    // Проверка на точную длину серии и номера паспорта
                    if (values[0].Length == 4 && values[1].Length == 6)
                    {
                        if (int.TryParse(values[0], out var passpSeries) && int.TryParse(values[1], out var passpNumber))
                        {
                            return (passpSeries, passpNumber);
                        }
                    }
                    // Если данные не валидны, возвращаем null
                    return null;
                }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount });

            var batchBlock = new BatchBlock<(int passpSeries, int passpNumber)?>(BatchSize);

            var filterBlock = new TransformBlock<(int passpSeries, int passpNumber)?[], (int passpSeries, int passpNumber)[]>(
                batch =>
                {
                    // Фильтруем только валидные значения
                    return batch.Where(x => x.HasValue).Select(x => x.Value).ToArray();
                });

            var insertBlock = new ActionBlock<(int passpSeries, int passpNumber)[]>(async batch =>
            {
                // Если батч содержит валидные данные, выполняем запись
                if (batch.Length > 0)
                {
                    await InsertBatchAsync(batch);
                }
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

            // Соединяем блоки
            readBlock.LinkTo(batchBlock);
            batchBlock.LinkTo(filterBlock, new DataflowLinkOptions { PropagateCompletion = true });
            filterBlock.LinkTo(insertBlock, new DataflowLinkOptions { PropagateCompletion = true });

            // Чтение файла и отправка данных в блоки
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        await readBlock.SendAsync(line);
                    }
                }
            }

            // Завершаем блоки после окончания чтения
            readBlock.Complete();
            await readBlock.Completion;
            batchBlock.Complete();
            await batchBlock.Completion;
            await filterBlock.Completion;
            await insertBlock.Completion;
        }

        private async Task InsertBatchAsync((int passpSeries, int passpNumber)[] batch)
        {
            using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            using (var conn = new NpgsqlConnection(dbContext.Database.GetDbConnection().ConnectionString))
            {
                await conn.OpenAsync();
                using (var writer = conn.BeginBinaryImport("COPY passports (passp_series, passp_number) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var item in batch)
                    {
                        writer.StartRow();
                        writer.Write(item.passpSeries);
                        writer.Write(item.passpNumber);
                    }
                    writer.Complete();
                }
            }
        }
    }

}
