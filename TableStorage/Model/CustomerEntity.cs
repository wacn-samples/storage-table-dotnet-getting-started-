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
namespace DataTableStorageSample.Model
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// 为演示Azure表服务定义一个自定义实体. 示例代码将客户的名字和姓氏分别用作行键和分区键，在现实的应用中可能不是一个很好的行键和分区键，因为它不能保证唯一的对应一个实体，行键和分区键
    /// 唯一对应一个实体是Azure表服务必须的。   
    /// <summary>
    public class CustomerEntity : TableEntity
    {
        //您的实体类型必须暴露一个无参数的构造函数     
        public CustomerEntity() { }

        // 定义行键和分区键
        public CustomerEntity(string lastName, string firstName)
        {
            this.PartitionKey = lastName;
            this.RowKey = firstName;
        }

        // 任何一个属性都应该被保存在Azure表服务中，属性必须是public类型，且同时有get和set方法 
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
