using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Compat.Core;
using Xunit;
using CoC.Web.SOM.Controllers;
using Core.DTO.Response;
using CoC.Web.SOM.Response;
using CoC.Web.SOM.Response.SOM;
using CoC.Web.SOM.Response.SOM.Order;
using Compat.Legacy.Cases;

namespace Compat.Legacy
{
    /// <summary>
    /// LEGACY MODE: runs each registered case against the .NET Framework 4.8 app and writes the
    /// approved golden snapshot under compat-tests/snapshots/. Re-running refreshes the approved
    /// snapshots.
    ///
    /// Run:  dotnet test compat-tests/Compat.Legacy
    ///
    /// AppConfigFixture pre-seeds AppConfig.Instance so that ApiEndPoint static field initializers
    /// succeed (they read AppConfig.Instance.*Url which would otherwise throw due to
    /// HttpRuntime.AppDomainAppPath being null in a host-less test process).
    ///
    /// THIS FILE IS THE PROJECT'S EXTENSION POINT (see compat-tests/Compat.Legacy/Cases/README.md):
    /// out of the box there are no registered controllers/cases, so the theory below is Skip-annotated
    /// and `dotnet test` reports it as Skipped (not failed, not silently absent). Wire up your own
    /// controllers/cases and delete the Skip to activate real snapshot generation.
    /// </summary>
    [Collection("LegacyCompat")]
    public class LegacySnapshotGenerationTests
    {
        private const string NoCasesRegisteredReason =
            "No project-specific cases are registered yet. Add a Cases/<Feature>Cases.cs file, " +
            "register its controller in BuildHost() and its cases in Cases() below, then remove " +
            "this Skip. See compat-tests/Compat.Legacy/Cases/README.md.";

        private static string SnapshotDir =>
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "snapshots"));

        /// <summary>
        /// EXTENSION POINT: register one controller factory per line, e.g.:
        ///   .Register("Widgets", api => new WidgetsController(api, /* other deps: null unless the action uses them */))
        /// Non-IApiHelper deps can usually be null unless the exercised action actually uses them.
        /// </summary>
        private static LegacyCompatibilityHost BuildHost()
        {
            return new LegacyCompatibilityHost()
                .Register("AgencyCustomers", api =>
                {
                    api.CannedResponse = (endpoint, type) =>
                    {
                        if (endpoint.Contains("Search"))
                        {
                            return new PagingResponse<AgencyCustomerSearchRes>
                            {
                                TotalRecord = 1,
                                Records = new List<AgencyCustomerSearchRes>
                                {
                                    new AgencyCustomerSearchRes
                                    {
                                        AgencyID = 5,
                                        AgencyName = "Test Agency",
                                        Phone = 0909123456,
                                        TaxNo = "123456789",
                                        Address = "123 Main St",
                                        AgencyBranchID = 1,
                                        AgencyBranchName = "Con cưng",
                                        CustomerTypeID = 2,
                                        CustomerTypeName = "Sakura",
                                        UpdatedUser = 1,
                                        UpdatedDate = new DateTime(2026, 7, 1),
                                        UpdatedUserName = "Admin",
                                        UpdatedFullName = "System Admin"
                                    }
                                }
                            };
                        }
                        if (endpoint.Contains("5"))
                        {
                            return new AgencyCustomerReadByIDRes
                            {
                                AgencyID = 5,
                                AgencyName = "Test Agency",
                                Phone = 0909123456,
                                TaxNo = "123456789",
                                Address = "123 Main St",
                                AgencyBranchID = 1,
                                AgencyBranchName = "Con cưng",
                                CustomerTypeID = 2,
                                CustomerTypeName = "Sakura",
                                CreatedUser = 1,
                                CreatedDate = new DateTime(2026, 7, 1),
                                CreatedUserName = "Admin",
                                CreatedFullName = "System Admin",
                                UpdatedUser = 1,
                                UpdatedDate = new DateTime(2026, 7, 1),
                                UpdatedUserName = "Admin",
                                UpdatedFullName = "System Admin"
                            };
                        }
                        return null;
                    };
                    return new AgencyCustomersController(api);
                })
                .Register("BufferGrandOrders", api =>
                {
                    api.CannedResponse = (endpoint, type) =>
                    {
                        if (endpoint.Contains("OrderTypes"))
                        {
                            return new List<OrderTypeRes>
                            {
                                new OrderTypeRes { OrderTypeID = 1, OrderTypeName = "Test Type", OutStockTypeID = 1 }
                            };
                        }
                        if (endpoint.Contains("OrderStatus"))
                        {
                            return new List<OrderStatusRes>
                            {
                                new OrderStatusRes { OrderStatusID = 1, OrderStatusName = "Mới tạo" }
                            };
                        }
                        if (endpoint.Contains("SupplierOrderTypes"))
                        {
                            return new List<SupplierOrderTypeRes>
                            {
                                new SupplierOrderTypeRes { IsInternal = false }
                            };
                        }
                        if (endpoint.Contains("SystemConfig"))
                        {
                            if (endpoint.Contains("SOM_SendOrderRotation_Mode"))
                            {
                                return new SystemConfigRes { ConfigValue = "1" };
                            }
                            return new SystemConfigRes { ConfigValue = "1,2,3" };
                        }
                        if (endpoint.Contains("ProductCategories"))
                        {
                            return new List<ProductCategoryRes>
                            {
                                new ProductCategoryRes { ParentCategoryID = 0, CategoryID = 1, CategoryName = "Category 1" }
                            };
                        }
                        if (endpoint.Contains("Stores"))
                        {
                            return new List<StoreRes>
                            {
                                new StoreRes { StoreID = 1, StoreName = "Test Store", IsDCStore = true, IsActived = true }
                            };
                        }
                        if (endpoint.Contains("StoreGroups") || endpoint.Contains("StoreGroup"))
                        {
                            return new List<StoreGroupRes>
                            {
                                new StoreGroupRes { StoreGroupID = 23, StoreGroupName = "Group 23", ParentStoreGroupID = 0 }
                            };
                        }
                        if (endpoint.Contains("ReasonChange"))
                        {
                            return new List<ReasonChangeRes>
                            {
                                new ReasonChangeRes { ReasonChangeID = 1, ReasonChange = "Reason 1" }
                            };
                        }
                        if (endpoint.Contains("Supplier"))
                        {
                            return new List<SupplierUserFullRes>
                            {
                                new SupplierUserFullRes { FullName = "Contact 1" }
                            };
                        }
                        if (endpoint.Contains("PricePurchases"))
                        {
                            return new List<PricePurchaseReadByProductRes>
                            {
                                new PricePurchaseReadByProductRes
                                {
                                    PID = 10,
                                    ProductID = "P001",
                                    ProductName = "Product 1",
                                    BuyPrice = 100,
                                    BuyPriceAfter = 100
                                }
                            };
                        }
                        if (endpoint.Contains("Products"))
                        {
                            return new List<ProductRes>
                            {
                                new ProductRes { PID = 10, ProductID = "P001", ProductName = "Product 1", CategoryID = 1 }
                            };
                        }
                        if (endpoint.Contains("BufferGrandOrders"))
                        {
                            if (endpoint.Contains("Search"))
                            {
                                return new PagingResponse<BufferGrandOrderSearchRes>
                                {
                                    TotalRecord = 1,
                                    Records = new List<BufferGrandOrderSearchRes>
                                    {
                                        new BufferGrandOrderSearchRes
                                        {
                                            OrderID = "BGO001",
                                            OrderStatusName = "Mới tạo",
                                            StoreName = "DC Test",
                                            OrderTypeName = "Test Type",
                                            StoreGroupName = "Group 23",
                                            ExpectedInputDate = new DateTime(2026, 7, 1),
                                            TotalOrderVolume = 10,
                                            TotalBoxOrder = 5,
                                            TotalConfirmOrderBox = 5,
                                            TotalConfirmOrderQuantity = 50,
                                            CreatedFullName = "System Admin",
                                            MerchandiseDescription = "Test description",
                                            CreatedDate = new DateTime(2026, 7, 1),
                                            ReasonChangeContent = "None",
                                            ReasonReInitContent = "None"
                                        }
                                    }
                                };
                            }
                            if (endpoint.Contains("Exports"))
                            {
                                return new BufferGrandOrderExportsRes
                                {
                                    Summarys = new List<BufferGrandOrderExportsSummaryRes>
                                    {
                                        new BufferGrandOrderExportsSummaryRes
                                        {
                                            OrderID = "BGO001",
                                            OrderStatusName = "Mới tạo",
                                            DCStoreName = "DC Test",
                                            OrderTypeName = "Test Type",
                                            StoreGroupName = "Group 23",
                                            ExpectedInputDate = new DateTime(2026, 7, 1),
                                            TotalOrderVolume = 10,
                                            TotalBoxOrder = 5,
                                            TotalConfirmOrderBox = 5,
                                            TotalConfirmOrderQuantity = 50,
                                            CreatedFullName = "System Admin",
                                            MerchandiseDescription = "Test description",
                                            CreatedDate = new DateTime(2026, 7, 1),
                                            ReasonChangeContent = "None",
                                            ReasonReInitContent = "None"
                                        }
                                    },
                                    Details = new List<BufferGrandOrderExportsDetailRes>
                                    {
                                        new BufferGrandOrderExportsDetailRes
                                        {
                                            OrderID = "BGO001",
                                            ProductID = "P001",
                                            ReferenceID = "R001",
                                            ProductName = "Product 1",
                                            DistributedStoreName = "Store 1",
                                            TotalOrderVolume = 2,
                                            TotalBoxOrder = 1,
                                            TotalConfirmOrderBox = 1,
                                            TotalConfirmOrderQuantity = 10
                                        }
                                    }
                                };
                            }
                            if (endpoint.Contains("Init"))
                            {
                                return new OrderInitRes { OID = 100, OrderID = "BGO001" };
                            }
                            if (endpoint.Contains("Confirm") || endpoint.Contains("ConfirmFinal"))
                            {
                                if (type == typeof(OrderConfirmRes))
                                {
                                    return new OrderConfirmRes { IsSuccess = true };
                                }
                                if (type == typeof(OrderConfirmFinalRes))
                                {
                                    return new OrderConfirmFinalRes { IsSuccess = true };
                                }
                                return new OrderConfirmRes { IsSuccess = true };
                            }
                            if (endpoint.Contains("ImportExcelBuyer"))
                            {
                                return new List<OrderDetailReadByIDRes>
                                {
                                    new OrderDetailReadByIDRes
                                    {
                                        PID = 10,
                                        ProductID = "P001",
                                        ProductName = "Product 1",
                                        OrderDistributeds = new List<OrderDistributedReadByIDRes>
                                        {
                                            new OrderDistributedReadByIDRes { StoreID = 1, StoreName = "Test Store", OrderBoxNumber = 5 }
                                        }
                                    }
                                };
                            }
                            if (endpoint.Contains("100"))
                            {
                                return new OrderReadByIDRes
                                {
                                    OID = 100,
                                    OrderID = "BGO001",
                                    StoreID = 1,
                                    OrderStatusID = 1,
                                    OrderDetails = new List<OrderDetailReadByIDRes>
                                    {
                                        new OrderDetailReadByIDRes
                                        {
                                            PID = 10,
                                            ProductID = "P001",
                                            ProductName = "Product 1",
                                            BoxNumber = 5,
                                            NumOfBox = 10,
                                            OrderDistributeds = new List<OrderDistributedReadByIDRes>
                                            {
                                                new OrderDistributedReadByIDRes
                                                {
                                                    StoreID = 1,
                                                    StoreName = "Test Store",
                                                    OrderBoxNumber = 5,
                                                    OrderQuantity = 50,
                                                    PID = 10,
                                                    ProductID = "P001",
                                                    ProductName = "Product 1"
                                                }
                                            }
                                        }
                                    }
                                };
                            }
                        }
                        return null;
                    };
                    return new BufferGrandOrdersController(api);
                })
                .Register("BufferOrder2Sakuras", api =>
                {
                    api.CannedResponse = (endpoint, type) =>
                    {
                        if (endpoint.Contains("OrderTypes"))
                        {
                            return new List<OrderTypeRes>
                            {
                                new OrderTypeRes { OrderTypeID = 1, OrderTypeName = "Test Type", OutStockTypeID = 1 }
                            };
                        }
                        if (endpoint.Contains("OrderStatus"))
                        {
                            return new List<OrderStatusRes>
                            {
                                new OrderStatusRes { OrderStatusID = 1, OrderStatusName = "Mới tạo" }
                            };
                        }
                        if (endpoint.Contains("SupplierOrderTypes"))
                        {
                            return new List<SupplierOrderTypeRes>
                            {
                                new SupplierOrderTypeRes { IsInternal = false }
                            };
                        }
                        if (endpoint.Contains("SystemConfig"))
                        {
                            if (endpoint.Contains("SOM_OrderSakura_StoreID"))
                            {
                                return new SystemConfigRes { ConfigValue = "1" };
                            }
                            return new SystemConfigRes { ConfigValue = "1,2,3" };
                        }
                        if (endpoint.Contains("PaymentTypes"))
                        {
                            return new List<PaymentTypeRes>
                            {
                                new PaymentTypeRes { PaymentTypeID = 3, PaymentTypeName = "Test Payment" }
                            };
                        }
                        if (endpoint.Contains("OrderPaymentTypes"))
                        {
                            return new List<OrderPaymentTypeRes>
                            {
                                new OrderPaymentTypeRes { OrderPaymentTypeID = 3, OrderPaymentName = "Test Order Payment" }
                            };
                        }
                        if (endpoint.Contains("Supplier"))
                        {
                            return new List<SupplierUserFullRes>
                            {
                                new SupplierUserFullRes { FullName = "Contact 1" }
                            };
                        }
                        if (endpoint.Contains("OutputOrders") || endpoint.Contains("Read2SakuraBufferOrder"))
                        {
                            return new OutputOrderRead2SakuraBufferOrderRes
                            {
                                OOID = 1,
                                OutputOrderID = "OUT001",
                                TotalQuantity = 10,
                                OutputOrderDetails = new List<OutputOrderRead2SakuraBufferOrderDetailRes>
                                {
                                    new OutputOrderRead2SakuraBufferOrderDetailRes
                                    {
                                        PID = 10,
                                        ProductID = "P001",
                                        ProductName = "Product 1"
                                    }
                                }
                            };
                        }
                        if (endpoint.Contains("Products"))
                        {
                            return new List<ProductRes>
                            {
                                new ProductRes { PID = 10, ProductID = "P001", ProductName = "Product 1", CategoryID = 1 }
                            };
                        }
                        if (endpoint.Contains("BufferOrder2Sakuras"))
                        {
                            if (endpoint.Contains("Search"))
                            {
                                return new PagingResponse<OrderRes>
                                {
                                    TotalRecord = 1,
                                    Records = new List<OrderRes>
                                    {
                                        new OrderRes
                                        {
                                            OrderID = "BGO001",
                                            OrderTypeName = "Test Type",
                                            OrderStatusName = "Mới tạo",
                                            CreatedDate = new DateTime(2026, 7, 1)
                                        }
                                    }
                                };
                            }
                            if (endpoint.Contains("Init"))
                            {
                                return new OrderInitRes { OID = 100, OrderID = "BGO001" };
                            }
                            if (endpoint.Contains("Create") || endpoint.Contains("Cancel") || endpoint.Contains("Send2Supplier") || endpoint.Contains("UpdateExpectedDate") || endpoint.Contains("UpdateInputDate") || endpoint.Contains("Update"))
                            {
                                return true;
                            }
                            if (endpoint.Contains("ImportExcelBuyer"))
                            {
                                return new List<OrderDetailReadByIDRes>
                                {
                                    new OrderDetailReadByIDRes
                                    {
                                        PID = 10,
                                        ProductID = "P001",
                                        ProductName = "Product 1",
                                        NumOfBox = 5
                                    }
                                };
                            }
                            if (endpoint.Contains("100"))
                            {
                                return new OrderReadByIDRes
                                {
                                    OID = 100,
                                    OrderID = "BGO001",
                                    StoreID = 1,
                                    OrderStatusID = 1,
                                    OrderDetails = new List<OrderDetailReadByIDRes>
                                    {
                                        new OrderDetailReadByIDRes
                                        {
                                            PID = 10,
                                            ProductID = "P001",
                                            ProductName = "Product 1",
                                            BoxNumber = 5,
                                            NumOfBox = 10
                                        }
                                    }
                                };
                            }
                        }
                        return null;
                    };
                    return new BufferOrder2SakurasController(api);
                });
        }

        /// <summary>
        /// EXTENSION POINT: aggregate every project cases file's All() here, e.g.:
        ///   foreach (var c in Compat.Legacy.Cases.WidgetsCases.All()) yield return new object[] { c };
        /// </summary>
        public static IEnumerable<object[]> Cases()
        {
            foreach (var c in AgencyCustomersCases.All()) yield return new object[] { c };
            foreach (var c in BufferGrandOrdersCases.All()) yield return new object[] { c };
            foreach (var c in BufferOrder2SakurasCases.All()) yield return new object[] { c };
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public async Task Generate_legacy_snapshot(CompatibilityCase testCase)
        {
            var store = new SnapshotStore(SnapshotDir);
            var runner = new CompatibilityRunner(store, legacy: BuildHost());

            var text = await runner.GenerateLegacySnapshotAsync(testCase);

            Assert.False(string.IsNullOrWhiteSpace(text));
            Assert.True(store.HasApproved(testCase.Feature, testCase.Key),
                $"Approved snapshot not written for feature '{testCase.Feature}', key '{testCase.Key}'");
        }
    }
}
