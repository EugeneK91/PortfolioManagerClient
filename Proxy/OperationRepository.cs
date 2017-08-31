using ORM;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy
{
    class OperationRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<PortfolioOperation> _operationItems;

        public OperationRepository()
        {
            _context = new PortfolioDbModel();
            _operationItems = _context.Set<PortfolioOperation>();
        }

        private List<PortfolioOperation> _portfolioItemViewModel = new List<PortfolioOperation>();

        public PortfolioOperation Create(PortfolioOperation item)
        {
            var temp = _operationItems.Add(item);
            SaveChanges();
            return temp;
        }

        public void Delete(int id)
        {
            var model = _operationItems.FirstOrDefault(c => c.Id == id);
            _operationItems.Remove(model);
            SaveChanges();
        }

        public void Edit(PortfolioOperation item)
        {
            var model = _operationItems.FirstOrDefault(c => c.Id == item.Id);
            model.Symbol = item.Symbol;
            model.SharesNumber = item.SharesNumber;
            model.UserId = item.UserId;
            SaveChanges();
        }

        public IEnumerable<PortfolioOperation> GetAll()
        {
            return _operationItems;
        }


        private void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
