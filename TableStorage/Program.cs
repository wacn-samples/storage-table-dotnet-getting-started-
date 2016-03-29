//----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//----------------------------------------------------------------------------------

namespace DataTableStorageSample
{
    using DataTableStorageSample.Model;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// Azure 表服务示例 - 演示如何使用Azure表存储执行普通的任务，示例包括创建表、增删改查操作，批量操作和不同的查询技术    
    /// 
    /// 注意：这个示例使用.NET 4.5异步编程模型来演示如何使用storage client libraries异步API调用存储服务。 在实际的应用中这种方式
    /// 可以提高程序的响应速度。调用存储服务只要添加关键字await为前缀即可。
    /// 
    /// 参考文档: 
    /// - 什么是存储账号- https://www.azure.cn/documentation/articles/storage-create-storage-account/
    /// - 表服务起步 - http://www.azure.cn/documentation/articles/storage-dotnet-how-to-use-tables/
    /// - 表服务概念 - https://msdn.microsoft.com/zh-cn/library/dd179463.aspx
    /// - 表服务 REST API - https://msdn.microsoft.com/zh-cn/library/dd179423.aspx
    /// - 表服务 C# API - http://go.microsoft.com/fwlink/?LinkID=398944
    /// - 存储模拟器 - https://www.azure.cn/documentation/articles/storage-use-emulator/
    /// - 使用 Async 和 Await异步编程  - http://msdn.microsoft.com/zh-cn/library/hh191443.aspx
    /// </summary>

    public class Program
    {
        // *************************************************************************************************************************
        // 使用说明: 这个示例可以在Azure存储模拟器（存储模拟器是Azure SDK安装的一部分）上运行，或者通过修改App.Config文档中的存储账号和存储密匙
        // 的方式针对存储服务来使用。      
        // 
        // 使用Azure存储模拟器来运行这个示例  (默认选项)
        //      1. 点击开始按钮或者是键盘的Windows键，然后输入“Azure Storage Emulator”来寻找Azure存储模拟器，之后点击运行。       
        //      2. 设置断点，然后使用F10按钮运行这个示例. 
        // 
        // 使用Azure存储服务来运行这个示例
        //      1. 打来AppConfig文件然后使用第二个连接字符串。
        //      2. 在Azure门户网站上创建存储账号，然后修改App.Config的存储账号和存储密钥。更多详细内容请阅读：https://www.azure.cn/documentation/articles/storage-dotnet-how-to-use-blobs/
        //      3. 设置断点，然后使用F10按钮运行这个示例. 
        // 
        // *************************************************************************************************************************
        internal const string TableName = "customer";

        public static void Main(string[] args)
        {
            Console.WriteLine("Azure 表存储示例\n");

            // 创建一个新表或者引用已经存在的表
            CloudTable table = CreateTableAsync().Result;

            // 演示增删改查操作
            BasicTableOperationsAsync(table).Wait();

            // 演示高级的功能，例如批量操作和分段多实体查询 
            AdvancedTableOperationsAsync(table).Wait();

            // 当您删除一个表然后创建同名的表时需要几秒的时间 - 所以为了让您快速成功的运行这个示例，我们创建的表没有进行删除。如果您需要删除表请去掉下面代码的注释             
            //DeleteTableAsync(table).Wait();

            Console.WriteLine("按任意键退出");
            Console.Read();
        }

        /// <summary>
        /// 创建一个表，并输入信息
        /// </summary>
        /// <returns>一个CloudTable对象</returns>
        private static async Task<CloudTable> CreateTableAsync()
        {
            // 通过连接字符串找到存储账号的信息
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // 创建一个tableClient莱赫表服务交互
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            Console.WriteLine("1. 创建一个表");
        
            CloudTable table = tableClient.GetTableReference(TableName);
            try
            {
                if (await table.CreateIfNotExistsAsync())
                {
                    Console.WriteLine("成功创建表: {0}", TableName);
                }
                else
                {
                    Console.WriteLine("表 {0} 已经存在", TableName);
                }
            }
            catch (StorageException)
            {
                Console.WriteLine("如果使用默认配置文件，请确保Azure模拟器已经启动。点击Windows键然后输入\"Azure Storage\"，找到Azure模拟器然后点击运行，之后请重新启动该示例.");
                Console.ReadLine();
                throw;
            }

            return table;
        }

        /// <summary>
        /// 演示增删改查操作 
        /// </summary>
        /// <param name="table">示例表</param>
        private static async Task BasicTableOperationsAsync(CloudTable table)
        {
            // 创建一个客户实体，关于这个实体的详细描述请看 Model\CustomerEntity.cs
            CustomerEntity customer = new CustomerEntity("Harp", "Walter")
            {
                Email = "Walter@contoso.com",
                PhoneNumber = "425-555-0101"
            };

            // 演示如何更新实体的电话信息
            Console.WriteLine("2. 使用InsertOrMerge更新插入操作来更新一个已存在的实体.");
            customer.PhoneNumber = "425-555-0105";
            customer = await InsertOrMergeEntityAsync(table, customer);

            // 演示如何通过单点查询读取已经更新完成的实体
            Console.WriteLine("3. 读取已经更新完成的实体.");
            customer = await RetrieveEntityUsingPointQueryAsync(table, "Harp", "Walter");

            // 演示如何删除实体
            Console.WriteLine("4. 删除实体. ");
            await DeleteEntityAsync(table, customer);
        }

        /// <summary>
        /// 演示高级的功能，例如批量操作和分段查询 
        /// </summary>
        /// <param name="table">示例表</param>
        private static async Task AdvancedTableOperationsAsync(CloudTable table)
        {
            // 演示批量更新或者插入的表操作
            Console.WriteLine("4. 批量插入实体. ");
            await BatchInsertOfCustomerEntitiesAsync(table);

            // 在一个区间内查询一定范围的数据
            Console.WriteLine("5. 检索姓Smith名字 >= 1 且 <= 75 的实体");
            await PartitionRangeQueryAsync(table, "Smith", "0001", "0075");

            // 查询一个分区内的所有数据
            Console.WriteLine("6. 检索姓Smith的所有实体.");
            await PartitionScanAsync(table, "Smith");
        }
        
        /// <summary>
        /// 验证App.Config文件中的连接字符串，当使用者没有更新有效的值时抛出错误提示
        /// </summary>
        /// <param name="storageConnectionString">连接字符串</param>
        /// <returns>CloudStorageAccount 对象</returns>
        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("提供的存储信息无效，请确认App.Config文件中的AccountName和AccountKey有效后重新启动该示例");
                Console.ReadLine();
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("提供的存储信息无效，请确认App.Config文件中的AccountName和AccountKey有效后重新启动该示例");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

        /// <summary>
        /// 表服务支持两种主要的插入操作类型
        ///  1. Insert - 插入一个新实体。如果相同的分区键和行键的实体已经存在则抛出错误
        ///  2. Replace - 替换一个已经存在的实体。使用一个新的实体替换掉一个已经存在的实体
        ///  3. Insert or Replace - 如果实体不存在，则插入实体。如果实体存在则替换已经存在的实体
        ///  4. Insert or Merge - 如果实体不存在，则插入实体，如果实体存在，则将提供的实体于已存在的实体合并
        /// 
        /// </summary>
        /// <param name="table">示例表名</param>
        /// <param name="entity">要插入/合并的实体</param>
        /// <returns></returns>
        private static async Task<CustomerEntity> InsertOrMergeEntityAsync(CloudTable table, CustomerEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            // 创建插入/合并的表操作
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

            // 执行操作
            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
            CustomerEntity insertedCustomer = result.Result as CustomerEntity;
            return insertedCustomer;
        }

        /// <summary>
        /// 演示最有效的存储查询 - 单点查询 - 指定行键和分区键
        /// </summary>
        /// <param name="table">示例表名</param>
        /// <param name="partitionKey">分区键 - 例子中 - 姓名</param>
        /// <param name="rowKey">行键 - 例子中 - 名字</param>
        private static async Task<CustomerEntity> RetrieveEntityUsingPointQueryAsync(CloudTable table, string partitionKey, string rowKey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<CustomerEntity>(partitionKey, rowKey);
            TableResult result = await table.ExecuteAsync(retrieveOperation);
            CustomerEntity customer = result.Result as CustomerEntity;
            if (customer != null)
            {
                Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", customer.PartitionKey, customer.RowKey, customer.Email, customer.PhoneNumber);
            }

            return customer;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="table">示例表名</param>
        /// <param name="deleteEntity">需要删除的实体</param>
        private static async Task DeleteEntityAsync(CloudTable table, CustomerEntity deleteEntity)
        {
            if (deleteEntity == null)
            {
                throw new ArgumentNullException("deleteEntity");
            }

            TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
            await table.ExecuteAsync(deleteOperation);
        }

        /// <summary>
        /// 演示通过一次写入操作将一批实体插入表中。批处理操作的一些其他注意事项：
        ///  1. 你可以在同一批处理操作中执行更新、删除和插入操作。
        ///  2. 单个批处理操作最多可包含 100 个实体。
        ///  3. 单次批处理操作中的所有实体都必须具有相同的分区键。
        ///  4. 虽然可以将某个查询作为批处理操作执行，但该操作必须是批处理中仅有的操作。
        ///  5. 批处理大小必须 <=4MB    
        /// </summary>
        /// <param name="table">示例表名</param>
        private static async Task BatchInsertOfCustomerEntitiesAsync(CloudTable table)
        {
            // 创建批量操作
            TableBatchOperation batchOperation = new TableBatchOperation();

            // 下面的代码生成查询示例中使用的测试数据
            for (int i = 0; i < 100; i++)
            {
                batchOperation.InsertOrMerge(new CustomerEntity("Smith", string.Format("{0}", i.ToString("D4")))
                {
                    Email = string.Format("{0}@contoso.com", i.ToString("D4")),
                    PhoneNumber = string.Format("425-555-{0}", i.ToString("D4"))
                });
            }

            // 执行批量操作
            IList<TableResult> results = await table.ExecuteBatchAsync(batchOperation);
            foreach (var res in results)
            {
                var customerInserted = res.Result as CustomerEntity;
                Console.WriteLine("插入实体其 \t Etag = {0} and PartitionKey = {1}, RowKey = {2}", customerInserted.ETag, customerInserted.PartitionKey, customerInserted.RowKey);
            }

        }

        /// <summary>
        /// 演示分区范围查询，我们在分区中查询一个特定范围的实体结合。异步的API需要用户使用continuation令牌实现分页      
        /// </summary>
        /// <param name="table">示例表名</param>
        /// <param name="partitionKey">指定分区键，在其中搜寻实体</param>
        /// <param name="startRowKey">指定用于搜索的行键范围的最低边界</param>
        /// <param name="endRowKey">指定用于搜索的行键的最高边界</param>
        private static async Task PartitionRangeQueryAsync(CloudTable table, string partitionKey, string startRowKey, string endRowKey)
        {
            // 使用fluid API创建范围查询
            TableQuery<CustomerEntity> rangeQuery = new TableQuery<CustomerEntity>().Where(
                TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startRowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, endRowKey))));

            // 结果分页展示 - 每次请求从服务器取50条结果
            TableContinuationToken token = null;
            rangeQuery.TakeCount = 50;
            do
            {
                TableQuerySegment<CustomerEntity> segment = await table.ExecuteQuerySegmentedAsync(rangeQuery, token);
                token = segment.ContinuationToken;
                foreach (CustomerEntity entity in segment)
                {
                    Console.WriteLine("Customer: {0},{1}\t{2}\t{3}", entity.PartitionKey, entity.RowKey, entity.Email, entity.PhoneNumber);
                }
            }
            while (token != null);
        }

        /// <summary>
        /// 演示在一个分区内检索所有的实体。注意这不是一个效率的范围检索 - 但肯定比一个全表检索更有效。异步的API需要用户使用continuation令牌实现分页 
        /// </summary>
        /// <param name="table">示例表名</param>
        /// <param name="partitionKey">指定分区键，在其中搜寻实体</param>
        private static async Task PartitionScanAsync(CloudTable table, string partitionKey)
        {
            TableQuery<CustomerEntity> partitionScanQuery = new TableQuery<CustomerEntity>().Where
                (TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            TableContinuationToken token = null;
            // 分页展示数据
            do
            {
                TableQuerySegment<CustomerEntity> segment = await table.ExecuteQuerySegmentedAsync(partitionScanQuery, token);
                token = segment.ContinuationToken;
                foreach (CustomerEntity entity in segment)
                {
                    Console.WriteLine("Customer: {0},{1}\t{2}\t{3}", entity.PartitionKey, entity.RowKey, entity.Email, entity.PhoneNumber);
                }
            }
            while (token != null);
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="table">示例表名</param>
        private static async Task DeleteTableAsync(CloudTable table)
        {
            await table.DeleteIfExistsAsync();
        }
    }
}
