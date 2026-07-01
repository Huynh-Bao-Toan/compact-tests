using System.Collections.Generic;
using Compat.Core;

namespace Compat.Legacy.Cases
{
    public static class AgencyCustomersCases
    {
        public static IEnumerable<CompatibilityCase> All()
        {
            // 1. Index View
            yield return new CompatibilityCase
            {
                Feature = "AgencyCustomers",
                Key = CompatibilityCaseKey.For("Index", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/AgencyCustomers/Index"
            };

            // 2. IndexOutputCache View
            yield return new CompatibilityCase
            {
                Feature = "AgencyCustomers",
                Key = CompatibilityCaseKey.For("IndexOutputCache", CompatibilityScenarios.PascalCase),
                Method = "GET",
                Url = "/AgencyCustomers/IndexOutputCache"
            };

            // 3. Search - FormUrlEncoded (normal binding)
            yield return new CompatibilityCase
            {
                Feature = "AgencyCustomers",
                Key = CompatibilityCaseKey.For("Search", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/AgencyCustomers/Search",
                ContentType = "application/x-www-form-urlencoded",
                Body = "KeySearch=test&AgencyBranchID=1&CustomerTypeID=2&PageSize=10&PageIndex=1"
            };

            // 4. Search - NumberAsString
            yield return new CompatibilityCase
            {
                Feature = "AgencyCustomers",
                Key = CompatibilityCaseKey.For("Search", CompatibilityScenarios.NumberAsString),
                Method = "POST",
                Url = "/AgencyCustomers/Search",
                ContentType = "application/x-www-form-urlencoded",
                Body = "KeySearch=test&AgencyBranchID=-1&CustomerTypeID=2&PageSize=10&PageIndex=1"
            };

            // 5. ReadByID
            yield return new CompatibilityCase
            {
                Feature = "AgencyCustomers",
                Key = CompatibilityCaseKey.For("ReadByID", CompatibilityScenarios.PascalCase),
                Method = "POST",
                Url = "/AgencyCustomers/ReadByID?id=5",
                ContentType = "application/x-www-form-urlencoded",
                Body = ""
            };

            // 6. Create
            yield return new CompatibilityCase
            {
                Feature = "AgencyCustomers",
                Key = CompatibilityCaseKey.For("Create", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/AgencyCustomers/Create",
                ContentType = "application/x-www-form-urlencoded",
                Body = "AgencyName=DaiLyTest&Phone=909123456&TaxNo=123456789&Address=HCM&AgencyBranchID=1&CustomerTypeID=2"
            };

            // 7. Update
            yield return new CompatibilityCase
            {
                Feature = "AgencyCustomers",
                Key = CompatibilityCaseKey.For("Update", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/AgencyCustomers/Update",
                ContentType = "application/x-www-form-urlencoded",
                Body = "AgencyID=5&AgencyName=DaiLyUpdate&Phone=909123456&TaxNo=123456789&Address=HCM&AgencyBranchID=1&CustomerTypeID=2"
            };

            // 8. Delete
            yield return new CompatibilityCase
            {
                Feature = "AgencyCustomers",
                Key = CompatibilityCaseKey.For("Delete", CompatibilityScenarios.PascalCase),
                Method = "POST",
                Url = "/AgencyCustomers/Delete?id=5",
                ContentType = "application/x-www-form-urlencoded",
                Body = ""
            };

            // 9. ExportExcel
            yield return new CompatibilityCase
            {
                Feature = "AgencyCustomers",
                Key = CompatibilityCaseKey.For("ExportExcel", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/AgencyCustomers/ExportExcel",
                ContentType = "application/x-www-form-urlencoded",
                Body = "KeySearch=test&AgencyBranchID=1&CustomerTypeID=2&PageSize=10&PageIndex=1"
            };
        }
    }
}
