using System.Collections.Generic;
using System.Web.Http;
using PortfolioManagerClient.Models;
using PortfolioManagerClient.Services;
using Proxy;
using PortfolioManagerClient.Converters;
using System.Net;
using System;

namespace PortfolioManagerClient.Controllers
{
    /// <summary>
    /// Processes portfolio item requests.
    /// </summary>
    public class PortfolioItemsController : ApiController
    {
        private readonly PortfolioItemsService _portfolioItemsService = new PortfolioItemsService();
        private readonly UsersService _usersService = new UsersService();
        private List<PortfolioItemViewModel> _portfolioItemViewModel = new List<PortfolioItemViewModel>();
        private MediatorService _mediatorService ;
        

        public PortfolioItemsController()
        {
            _mediatorService =  new MediatorService(_usersService.GetOrCreateUser());
          //  _portfolioItemViewModel = _mediatorService.GetAllLocal().PortfolioItemsToPortfolioItemViewModels();
        }
        /// <summary>
        /// Returns all portfolio items for the current user.
        /// </summary>
        /// <returns>The list of portfolio items.</returns>
        [Route ("api/PortfolioItems/GetLocal")]
        public IList<PortfolioItemViewModel> GetLocal()
        {

            var userId = _usersService.GetOrCreateUser();
            
            // return _portfolioItemsService.GetItems(userId).PortfolioItemsToPortfolioItemViewModels();
            return _mediatorService.GetAllLocal().PortfolioItemsToPortfolioItemViewModels();
        }

        [Route("api/PortfolioItems/GetRemote")]
        public void GetRemote()
        {

            var userId = _usersService.GetOrCreateUser();

            // return _portfolioItemsService.GetItems(userId).PortfolioItemsToPortfolioItemViewModels();
             _mediatorService.GetAllRemote();//.PortfolioItemsToPortfolioItemViewModels();
        }

        [Route("api/PortfolioItems/GetStocks")]
        public List<string> GetStocks(string symbols)
        {
            string csvData = string.Empty;

            using (WebClient web = new WebClient())
            {
                var t = $"http://finance.yahoo.com/d/quotes.csv?s={symbols}&f=sl1";
                csvData = web.DownloadString($"http://finance.yahoo.com/d/quotes.csv?s={symbols}&f=sl1");
            }
            
            var  arrPrice = new List<string>() ;
            GetPrices(csvData,arrPrice);
            return arrPrice;
        }


        [Route("api/PortfolioItems/GetStockPrices")]
        public List<object> GetStockPrices(string symbol)
        {
            string csvData = string.Empty;
            // Compose the URL.
            string url = $"http://www.google.com/finance/historical?output=csv&q={symbol}";

            // Get the web response.
            using (WebClient web = new WebClient())
            {
                try
                {
                    csvData = web.DownloadString(url);
                }
                catch (Exception ex)
                {

                    return null;
                }
            }


            // Get the historical prices.
            string[] lines = csvData.Split(
                new char[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);
            var prices = new List<object>();
            // Process the lines, skipping the header.
            for (int i = lines.Length-1; i >= 1; i--)
            {
                string line = lines[i];

                prices.Add(new {year= Convert.ToDateTime(line.Split(',')[0]), value= line.Split(',')[4]});
            }


            return prices;//.GetRange(0,30);
        }

        [Route("api/PortfolioItems/GetOperation")]
        public IList<ORM.PortfolioOperation> GetOperation()
        {
            var userId = _usersService.GetOrCreateUser();
            return _mediatorService.GetOperations();
        }

        /// <summary>
        /// Updates the existing portfolio item.
        /// </summary>
        /// <param name="portfolioItem">The portfolio item to update.</param>
        public void Put(PortfolioItemViewModel portfolioItem)
        {
            _mediatorService.Edit(portfolioItem.PortfolioItemViewModelToPortfolioItem());
            //portfolioItem.UserId = _usersService.GetOrCreateUser();
            //_portfolioItemsService.UpdateItem(portfolioItem.PortfolioItemViewModelToPortfolioItem());
        }

        /// <summary>
        /// Deletes the specified portfolio item.
        /// </summary>
        /// <param name="id">The portfolio item identifier.</param>
        public void Delete(int id)
        {
            _mediatorService.Delete(id);
            //_portfolioItemsService.DeleteItem(id);
        }

        /// <summary>
        /// Creates a new portfolio item.
        /// </summary>
        /// <param name="portfolioItem">The portfolio item to create.</param>
        public void Post(PortfolioItemViewModel portfolioItem)
        {
          //  _portfolioItemsService.CreateItem(portfolioItem.PortfolioItemViewModelToPortfolioItem());

            _mediatorService.Create(portfolioItem.PortfolioItemViewModelToPortfolioItem());
        }

        [NonAction]
        private void GetPrices(string csvData,List<string> arrPrice)
        {
            string[] rows = csvData.Replace("\r", "").Split('\n');
            foreach (string row in rows)
            {
                if (string.IsNullOrEmpty(row)) continue;
                string[] cols = row.Split(',');

                arrPrice.Add(cols[1]);
            }
        } 
    }
}
