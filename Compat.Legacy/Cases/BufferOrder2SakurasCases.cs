using System.Collections.Generic;
using Compat.Core;

namespace Compat.Legacy.Cases
{
    public static class BufferOrder2SakurasCases
    {
        public static IEnumerable<CompatibilityCase> All()
        {
            // 1. Index
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("Index", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferOrder2Sakuras/Index"
            };

            // 2. IndexOutputCache
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("IndexOutputCache", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferOrder2Sakuras/IndexOutputCache"
            };

            // 3. IndexOutputCacheData
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("IndexOutputCacheData", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferOrder2Sakuras/IndexOutputCacheData"
            };

            // 4. Search
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("Search", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/Search",
                ContentType = "application/x-www-form-urlencoded",
                Body = "KeySearch=test&PageIndex=1&PageSize=10"
            };

            // 5. ExportExcel
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("ExportExcel", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/ExportExcel",
                ContentType = "application/x-www-form-urlencoded",
                Body = "KeySearch=test&PageIndex=1&PageSize=10"
            };

            // 6. Create
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("Create", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferOrder2Sakuras/Create"
            };

            // 7. CreateOutputCache
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("CreateOutputCache", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferOrder2Sakuras/CreateOutputCache"
            };

            // 8. CreateOutputCacheData
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("CreateOutputCacheData", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferOrder2Sakuras/CreateOutputCacheData"
            };

            // 9. Init
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("Init", CompatibilityScenarios.PascalCase),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/Init?ID=1",
                ContentType = "application/x-www-form-urlencoded",
                Body = ""
            };

            // 10. CheckSupplierOrder
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("CheckSupplierOrder", CompatibilityScenarios.PascalCase),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/CheckSupplierOrder?SupplierID=1",
                ContentType = "application/x-www-form-urlencoded",
                Body = ""
            };

            // 11. CheckOutputOrder
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("CheckOutputOrder", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/CheckOutputOrder",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OutputOrderID=OUT001&OrderTypeID=1"
            };

            // 12. Create (POST)
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("CreatePost", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/Create",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderTypeID=1&ExpectedInputDate=2026-07-01&StoreID=1&SupplierID=1&OutputOrderID=OUT001"
            };

            // 13. ExportExcelDetail (POST)
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("ExportExcelDetailPost", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/ExportExcelDetail",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&OrderTypeID=1&OrderDetails%5B0%5D.ProductID=P001&OrderDetails%5B0%5D.ProductName=Product+1&OrderDetails%5B0%5D.NumOfBox=5"
            };

            // 14. ImportExcel
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("ImportExcel", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/ImportExcel",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderType=1&Path=temp.xlsx"
            };

            // 15. GetProductCategoryAdjustPrice
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("GetProductCategoryAdjustPrice", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/GetProductCategoryAdjustPrice",
                ContentType = "application/x-www-form-urlencoded",
                Body = "BusinessType=Retail"
            };

            // 16. GetProduct
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("GetProduct", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/GetProduct",
                ContentType = "application/x-www-form-urlencoded",
                Body = "ProductID=P001&SupplierID=1&OrderType=1"
            };

            // 17. CheckPrice
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("CheckPrice", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/CheckPrice",
                ContentType = "application/x-www-form-urlencoded",
                Body = "PID=10&ProductID=P001&OutputTypeID=1"
            };

            // 18. Edit
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("Edit", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferOrder2Sakuras/Edit?ID=100&isACC=false"
            };

            // 19. EditOutputCache
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("EditOutputCache", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferOrder2Sakuras/EditOutputCache"
            };

            // 20. EditOutputCacheData
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("EditOutputCacheData", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferOrder2Sakuras/EditOutputCacheData"
            };

            // 21. UpdateExpectedDate
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("UpdateExpectedDate", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/UpdateExpectedDate",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&ExpectedInputDate=2026-07-02"
            };

            // 22. Cancel
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("Cancel", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/Cancel",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&ReasonID=1"
            };

            // 23. SendOrder
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("SendOrder", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/SendOrder",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&SupplierID=1"
            };

            // 24. UpdateInputDate
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("UpdateInputDate", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/UpdateInputDate",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&ExpectedInputDate=2026-07-02"
            };

            // 25. ExportExcelDetail (GET)
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("ExportExcelDetailGet", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/BufferOrder2Sakuras/ExportExcelDetail?ID=100"
            };

            // 26. Update
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("Update", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/Update",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&OrderTypeID=1"
            };

            // 27. ExportExcelTemplateEditFOC
            yield return new CompatibilityCase
            {
                Feature = "BufferOrder2Sakuras",
                Key = CompatibilityCaseKey.For("ExportExcelTemplateEditFOC", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/BufferOrder2Sakuras/ExportExcelTemplateEditFOC",
                ContentType = "application/x-www-form-urlencoded",
                Body = "OrderID=BGO001&OrderStatusID=1"
            };
        }
    }
}
