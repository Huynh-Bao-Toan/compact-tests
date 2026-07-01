using System.Collections.Generic;
using Compat.Core;

namespace Compat.Legacy.Cases
{
    public static class BufferGrandOrdersCases
    {
        public static IEnumerable<CompatibilityCase> All()
        {
            // 1. Index
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("Index", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferGrandOrders/Index"
            };

            // 2. IndexOutputCache
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("IndexOutputCache", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferGrandOrders/IndexOutputCache"
            };

            // 3. IndexOutputCacheData
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("IndexOutputCacheData", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferGrandOrders/IndexOutputCacheData"
            };

            // 4. Search
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("Search", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/Search",
                ContentType = "application/x-www-form-urlencoded",
                Body = "KeySearch=test&PageIndex=1&PageSize=10"
            };

            // 5. ExportExcel
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("ExportExcel", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/ExportExcel",
                ContentType = "application/x-www-form-urlencoded",
                Body = "KeySearch=test&PageIndex=1&PageSize=10"
            };

            // 6. Create
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("Create", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferGrandOrders/Create"
            };

            // 7. CreateOutputCache
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("CreateOutputCache", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferGrandOrders/CreateOutputCache"
            };

            // 8. CreateOutputCacheData
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("CreateOutputCacheData", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferGrandOrders/CreateOutputCacheData"
            };

            // 9. Create (POST)
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("CreatePost", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/Create",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderTypeID=1&ExpectedInputDate=2026-07-01&StoreID=1"
            };

            // 10. Edit
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("Edit", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferGrandOrders/Edit?id=100"
            };

            // 11. EditOutputCache
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("EditOutputCache", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferGrandOrders/EditOutputCache"
            };

            // 12. EditOutputCacheData
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("EditOutputCacheData", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferGrandOrders/EditOutputCacheData"
            };

            // 13. Init
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("Init", CompatibilityScenarios.PascalCase),
                Method = "POST",
                Url = "/BufferGrandOrders/Init?StoreID=1",
                ContentType = "application/x-www-form-urlencoded",
                Body = ""
            };

            // 14. SendOrder
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("SendOrder", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/SendOrder",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&SupplierID=1"
            };

            // 15. Confirm
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("Confirm", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/Confirm",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&OrderTypeID=1"
            };

            // 16. Cancel
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("Cancel", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/Cancel",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&ReasonID=1"
            };

            // 17. SearchStoreGroup
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("SearchStoreGroup", CompatibilityScenarios.PascalCase),
                Method = "POST",
                Url = "/BufferGrandOrders/SearchStoreGroup?ParentStoreGroupID=0",
                ContentType = "application/x-www-form-urlencoded",
                Body = ""
            };

            // 18. CheckPriceAndStock
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("CheckPriceAndStock", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/CheckPriceAndStock",
                ContentType = "application/x-www-form-urlencoded",
                Body = "PID=10&OutputTypeID=1&ProductID=P001&OrderTypeID=1&LstStore%5B0%5D.StoreID=1&NumOfBox=5&StoreID=1"
            };

            // 19. GetProduct
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("GetProduct", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/GetProduct",
                ContentType = "application/x-www-form-urlencoded",
                Body = "ProductID=P001"
            };

            // 20. ImportExcelDetail
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("ImportExcelDetail", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/ImportExcelDetail",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderType=1&Path=temp.xlsx"
            };

            // 21. CheckSupplierOrder
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("CheckSupplierOrder", CompatibilityScenarios.PascalCase),
                Method = "POST",
                Url = "/BufferGrandOrders/CheckSupplierOrder?SupplierID=1",
                ContentType = "application/x-www-form-urlencoded",
                Body = ""
            };

            // 22. ExportExcelDetail
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("ExportExcelDetail", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/ExportExcelDetail",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&OrderDetails%5B0%5D.ProductID=P001&OrderDetails%5B0%5D.ProductName=Product+1&OrderDetails%5B0%5D.OrderDistributeds%5B0%5D.StoreID=1&OrderDetails%5B0%5D.OrderDistributeds%5B0%5D.StoreName=Test+Store&OrderDetails%5B0%5D.OrderDistributeds%5B0%5D.OrderBoxNumber=5"
            };

            // 23. UpdateExpectedDate
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("UpdateExpectedDate", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/UpdateExpectedDate",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&ExpectedInputDate=2026-07-02"
            };

            // 24. Update
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("Update", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/Update",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&OrderTypeID=1"
            };

            // 25. CalculatingDistribution
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("CalculatingDistribution", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/CalculatingDistribution",
                ContentType = "application/x-www-form-urlencoded",
                Body = "objOrderDetail_Calculate.PID=10&objOrderDetail_Calculate.ProductID=P001&objOrderDetail_Calculate.ProductName=Product+1&objOrderDetail_Calculate.NumOfBox=10&NewOrderBox=5&OrderTypeID=1&StoreID=1"
            };

            // 26. ValidNumberInStock
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("ValidNumberInStock", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/ValidNumberInStock",
                ContentType = "application/x-www-form-urlencoded",
                Body = "CurrentQuantity=50&OrderTypeID=1&PID=10&ProductID=P001&StoreID=1"
            };

            // 27. ConfirmFinal
            yield return new CompatibilityCase
            {
                Feature = "BufferGrandOrders",
                Key = CompatibilityCaseKey.For("ConfirmFinal", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferGrandOrders/ConfirmFinal",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&OrderTypeID=1"
            };
        }
    }
}
