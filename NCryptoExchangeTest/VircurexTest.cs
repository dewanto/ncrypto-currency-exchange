﻿using Lostics.NCryptoExchange.Vircurex;
using Lostics.NCryptoExchange.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Lostics.NCryptoExchange
{
    [TestClass]
    public class VircurexTest
    {
        [TestMethod]
        public void TestBuildVircurexPublicUrl()
        {
            string url = VircurexExchange.BuildPublicUrl(VircurexExchange.Method.orderbook,
                VircurexExchange.Format.Json);

            Assert.AreEqual("https://api.vircurex.com/api/orderbook.json", url);
        }

        [TestMethod]
        public void TestParseVircurexCoins()
        {
            JObject jsonObj = LoadTestData<JObject>("get_currency_info.json");
            List<VircurexCurrency> currencies = VircurexCurrency.Parse(jsonObj);
            
            Assert.AreEqual(19, currencies.Count);

            Assert.AreEqual("BTC", currencies[0].CurrencyCode);
            Assert.AreEqual("Bitcoin", currencies[0].Label);
            Assert.AreEqual(4, currencies[0].Confirmations);

            Assert.AreEqual("NMC", currencies[1].CurrencyCode);
            Assert.AreEqual("Namecoin", currencies[1].Label);
        }

        [TestMethod]
        public void TestParseVircurexMarketData()
        {
            JObject marketsJson = LoadTestData<JObject>("get_info_for_currency.json");
            List<Market> markets = VircurexMarket.ParseMarkets(marketsJson);

            Assert.AreEqual(342, markets.Count);

            Assert.AreEqual(markets[0].Label, "ANC/BTC");
            Assert.AreEqual(markets[0].BaseCurrencyCode, "ANC");
            Assert.AreEqual(markets[0].Statistics.Volume24HBase, (decimal)301.90826656);
            Assert.AreEqual(markets[0].QuoteCurrencyCode, "BTC");
        }

        [TestMethod]
        public void TestParseVircurexMarketTrades()
        {
            JArray tradesJson = LoadTestData<JArray>("trades.json");
            VircurexMarketId marketId = new VircurexMarketId("DOGE", "BTC");
            List<MarketTrade> trades = VircurexParsers.ParseMarketTrades(marketId, tradesJson);

            Assert.AreEqual(1000, trades.Count);
            Assert.AreEqual("1110350", trades[0].TradeId.ToString());
            Assert.AreEqual(new DateTime(2014, 1, 3, 11, 14, 42), trades[0].DateTime);
            Assert.AreEqual((decimal)1208.12173, trades[0].Quantity);
            Assert.AreEqual((decimal)0.00000043, trades[0].Price);

            Assert.AreEqual("1110352", trades[1].TradeId.ToString());
        }

        [TestMethod]
        public void TestParseVircurexOrderBook()
        {
            JObject orderBookJson = LoadTestData<JObject>("orderbook.json");
            Book orderBook = VircurexParsers.ParseOrderBook(orderBookJson);
            List<MarketDepth> asks = orderBook.Asks;
            List<MarketDepth> bids = orderBook.Bids;

            Assert.AreEqual(asks[0].Price, (decimal)0.00000038);
            Assert.AreEqual(asks[0].Quantity, (decimal)156510.61595001);

            Assert.AreEqual(bids[0].Price, (decimal)0.00000037);
            Assert.AreEqual(bids[0].Quantity, (decimal)2295316.39314516);
        }

        [TestMethod]
        public void TestParseVircurexOrderBookAlt()
        {
            JObject orderBookJson = LoadTestData<JObject>("orderbook_alt.json");
            Dictionary<MarketId, Book> orderBooks = VircurexParsers.ParseMarketOrdersAlt("BTC",
                orderBookJson);
            Book orderBook = orderBooks[new VircurexMarketId("DGC", "BTC")];
            List<MarketDepth> asks = orderBook.Asks;
            List<MarketDepth> bids = orderBook.Bids;

            Assert.AreEqual(asks[0].Price, (decimal)0.00043586);
            Assert.AreEqual(asks[0].Quantity, (decimal)1.38879063);
            Assert.AreEqual(bids[0].Price, (decimal)0.00041001);
            Assert.AreEqual(bids[0].Quantity, (decimal)3.63826735);
        }

        private T LoadTestData<T>(string filename)
            where T : JToken
        {
            return TestUtils.LoadTestData<T>("Vircurex", filename);
        }
    }
}
