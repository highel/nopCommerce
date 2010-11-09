//------------------------------------------------------------------------------
// The contents of this file are subject to the nopCommerce Public License Version 1.0 ("License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at  http://www.nopCommerce.com/License.aspx. 
// 
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
// See the License for the specific language governing rights and limitations under the License.
// 
// The Original Code is nopCommerce.
// The Initial Developer of the Original Code is NopSolutions.
// All Rights Reserved.
// 
// Contributor(s): _______. 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.EntityClient;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Xml;
using Microsoft.Practices.Unity;
using NopSolutions.NopCommerce.BusinessLogic.Audit;
using NopSolutions.NopCommerce.BusinessLogic.Audit.UsersOnline;
using NopSolutions.NopCommerce.BusinessLogic.Caching;
using NopSolutions.NopCommerce.BusinessLogic.Categories;
using NopSolutions.NopCommerce.BusinessLogic.Configuration;
using NopSolutions.NopCommerce.BusinessLogic.Configuration.Settings;
using NopSolutions.NopCommerce.BusinessLogic.Content.Blog;
using NopSolutions.NopCommerce.BusinessLogic.Content.Forums;
using NopSolutions.NopCommerce.BusinessLogic.Content.NewsManagement;
using NopSolutions.NopCommerce.BusinessLogic.Content.Polls;
using NopSolutions.NopCommerce.BusinessLogic.Content.Topics;
using NopSolutions.NopCommerce.BusinessLogic.CustomerManagement;
using NopSolutions.NopCommerce.BusinessLogic.Data;
using NopSolutions.NopCommerce.BusinessLogic.Directory;
using NopSolutions.NopCommerce.BusinessLogic.Localization;
using NopSolutions.NopCommerce.BusinessLogic.Maintenance;
using NopSolutions.NopCommerce.BusinessLogic.Manufacturers;
using NopSolutions.NopCommerce.BusinessLogic.Measures;
using NopSolutions.NopCommerce.BusinessLogic.Media;
using NopSolutions.NopCommerce.BusinessLogic.Messages;
using NopSolutions.NopCommerce.BusinessLogic.Messages.SMS;
using NopSolutions.NopCommerce.BusinessLogic.Orders;
using NopSolutions.NopCommerce.BusinessLogic.Payment;
using NopSolutions.NopCommerce.BusinessLogic.Products;
using NopSolutions.NopCommerce.BusinessLogic.Products.Attributes;
using NopSolutions.NopCommerce.BusinessLogic.Products.Specs;
using NopSolutions.NopCommerce.BusinessLogic.Promo.Affiliates;
using NopSolutions.NopCommerce.BusinessLogic.Promo.Campaigns;
using NopSolutions.NopCommerce.BusinessLogic.Promo.Discounts;
using NopSolutions.NopCommerce.BusinessLogic.QuickBooks;
using NopSolutions.NopCommerce.BusinessLogic.Security;
using NopSolutions.NopCommerce.BusinessLogic.Shipping;
using NopSolutions.NopCommerce.BusinessLogic.Tax;
using NopSolutions.NopCommerce.BusinessLogic.Templates;
using NopSolutions.NopCommerce.BusinessLogic.Warehouses;

namespace NopSolutions.NopCommerce.BusinessLogic.IoC
{
    /// <summary>
    /// Inversion of Control factory implementation.
    /// This is a simple factory built with Microsoft Unity    
    /// </summary>
    public static class IoCFactory
    {
        #region Fields

        private static IDictionary<string, IUnityContainer> _containersDictionary;
        private static object _lock = new object();
        private static bool _initialized = false;
        #endregion

        #region Constructor

        /// <summary>
        /// Wires up the ioc containers
        /// </summary>        
        static IoCFactory()
        {
            InitContainers();
        }

        #endregion

        #region Utilities

        static void InitContainers()
        {
            if (_initialized == false)
            {
                lock (_lock)
                {
                    if (_initialized == false)
                    {
                        /*
                         * CREATE DICTIONARY
                         */
                        _containersDictionary = new Dictionary<string, IUnityContainer>();

                        /*
                         * CREATE CONTAINERS 
                         */
                        //Create root container
                        IUnityContainer rootContainer = new UnityContainer();
                        _containersDictionary.Add("RootContext", rootContainer);

                        //Create container for real context, child of root container
                        IUnityContainer realAppContainer = rootContainer.CreateChildContainer();
                        _containersDictionary.Add("RealAppContext", realAppContainer);

                        //Create container for testing, child of root container
                        IUnityContainer fakeAppContainer = rootContainer.CreateChildContainer();
                        _containersDictionary.Add("FakeAppContext", fakeAppContainer);

                        /*
                         * CONFIGURE CONTAINERS
                         */
                        ConfigureRootContainer(rootContainer);
                        ConfigureRealContainer(realAppContainer);
                        ConfigureFakeContainer(fakeAppContainer);

                        _initialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Configure root container.Register types and life time managers for unity builder process
        /// </summary>
        /// <param name="container">Container to configure</param>
        static void ConfigureRootContainer(IUnityContainer container)
        {
            //Take into account that Types and Mappings registration could be also done using the UNITY XML configuration
            //But we prefer doing it here (C# code) because we'll catch errors at compiling time instead execution time, if any type has been written wrong.

            //Register repositories mappings
            //to be done

            //Register default cache manager            
            //container.RegisterType<ICacheManager, NopRequestCache>(new PerExecutionContextLifetimeManager());
            
            //Register managers(services) mappings
            container.RegisterType<IOnlineUserService, OnlineUserService>(new PerExecutionContextLifetimeManager());            
            container.RegisterType<ISearchLogService, SearchLogService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ICustomerActivityService, CustomerActivityService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ILogService, LogService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ICategoryService, CategoryService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ISettingManager, SettingManager>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IBlogService, BlogService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IForumService, ForumService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<INewsService, NewsService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IPollService, PollService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ITopicService, TopicService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ICustomerService, CustomerService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ICountryService, CountryService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ICurrencyService, CurrencyService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ILanguageService, LanguageService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IStateProvinceService, StateProvinceService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ILocaleStringResourceManager, LocaleStringResourceManager>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IMaintenanceService, MaintenanceService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IManufacturerService, ManufacturerService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IMeasureService, MeasureService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IDownloadService, DownloadService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IPictureService, PictureService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ISMSService, SMSService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IMessageService, MessageService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IOrderService, OrderService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IShoppingCartService, ShoppingCartService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IPaymentService, PaymentService>(new PerExecutionContextLifetimeManager());            
            container.RegisterType<ICheckoutAttributeService, CheckoutAttributeService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IProductAttributeService, ProductAttributeService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ISpecificationAttributeService, SpecificationAttributeService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IProductService, ProductService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IAffiliateService, AffiliateService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ICampaignService, CampaignService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IDiscountService, DiscountService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IQBService, QBService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IACLService, ACLService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IBlacklistService, BlacklistService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IShippingByTotalService, ShippingByTotalService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IShippingByWeightAndCountryService, ShippingByWeightAndCountryService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IShippingByWeightService, ShippingByWeightService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IShippingService, ShippingService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ITaxCategoryService, TaxCategoryService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ITaxService, TaxService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ITaxProviderService, TaxProviderService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ITaxRateService, TaxRateService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<ITemplateService, TemplateService>(new PerExecutionContextLifetimeManager());
            container.RegisterType<IWarehouseService, WarehouseService>(new PerExecutionContextLifetimeManager());
        }

        /// <summary>
        /// Configure real container. Register types and life time managers for unity builder process
        /// </summary>
        /// <param name="container">Container to configure</param>
        static void ConfigureRealContainer(IUnityContainer container)
        {
            //Object context

            //Connection string
            var ecsbuilder = new EntityConnectionStringBuilder();
            ecsbuilder.Provider = "System.Data.SqlClient";
            ecsbuilder.ProviderConnectionString = NopConfig.ConnectionString;
            ecsbuilder.Metadata = @"res://*/Data.NopModel.csdl|res://*/Data.NopModel.ssdl|res://*/Data.NopModel.msl";
            string connectionString = ecsbuilder.ToString();
            InjectionConstructor connectionStringParam = new InjectionConstructor(connectionString);
            //Registering object context
            container.RegisterType<NopObjectContext>(new PerExecutionContextLifetimeManager(), connectionStringParam);
        }

        /// <summary>
        /// Configure fake container.Register types and life time managers for unity builder process
        /// </summary>
        /// <param name="container">Container to configure</param>
        static void ConfigureFakeContainer(IUnityContainer container)
        {
            //to be done
            //container.RegisterType(typeof(INopObjectContext), typeof(NopObjectFakeContext), new PerExecutionContextLifetimeManager());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns an injected object instance implementation for the requested interface
        /// IMPORTANT: It uses default IoC Container defined in AppSettings
        /// </summary>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// defaultIocContainer AppSetting key not found
        /// </exception>
        public static T Resolve<T>()
        {
            //We use the default container specified in AppSettings
            string containerName = ConfigurationManager.AppSettings["defaultIoCContainer"];
            return Resolve<T>(containerName);
        }
        
        /// <summary>
        /// Returns an injected implementation for the requested interface
        /// It uses provided IoC Container passed as parameter
        /// </summary>
        public static T Resolve<T>(string containerName)
        {
            IUnityContainer container = GetContainer(containerName);
            return container.Resolve<T>();
        }
        
        /// <summary>
        /// Returns an unity container
        /// IMPORTANT: It uses default IoC Container defined in AppSettings
        /// </summary>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// defaultIocContainer AppSetting key not found
        /// </exception>
        public static IUnityContainer GetContainer()
        {
            //We use the default container specified in AppSettings
            string containerName = ConfigurationManager.AppSettings["defaultIoCContainer"];
            return GetContainer(containerName);
        }

        /// <summary>
        /// Returns an unity container
        /// It uses provided IoC Container name passed as parameter
        /// </summary>
        public static IUnityContainer GetContainer(string containerName)
        {
            //check preconditions
            if (String.IsNullOrEmpty(containerName)
               ||
               String.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentNullException("Default containter name is not set");
            }

            if (!_containersDictionary.ContainsKey(containerName))
                throw new InvalidOperationException("IoC container is not found");

            IUnityContainer container = _containersDictionary[containerName];
            return container;
        }

        #endregion
    }
}
