﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prime.Common;
using Prime.Plugins.Services.Korbit;

namespace Prime.Tests.Providers
{
    [TestClass]
    public class KorbitTests : ProviderDirectTestsBase
    {
        public KorbitTests()
        {
            Provider = Networks.I.Providers.OfType<KorbitProvider>().FirstProvider();
        }

        [TestMethod]
        public override async Task TestGetAssetPairsAsync()
        {
            await base.TestGetAssetPairsAsync();
        }

        [TestMethod]
        public override async Task TestGetLatestPriceAsync()
        {
            PublicPairPriceContext = new PublicPairPriceContext(new AssetPair("BTC", "KRW"));

            await base.TestGetLatestPriceAsync();
        }

        [TestMethod]
        public override async Task TestGetOrderBookAsync()
        {
            OrderBookContext = new OrderBookContext(new AssetPair("BTC", "KRW"), 10);
            await base.TestGetOrderBookAsync();

            OrderBookContext = new OrderBookContext(new AssetPair("BTC", "KRW"));
            await base.TestGetOrderBookAsync();
        }

        [TestMethod]
        public override async Task TestGetLatestPricesAsync()
        {
            PublicAssetPricesContext = new PublicAssetPricesContext(new List<Asset>()
            {
                "KRW".ToAssetRaw()
            }, "BTC".ToAssetRaw());
            await base.TestGetLatestPricesAsync();
        }
    }
}
